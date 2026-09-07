# 10 — Idempotência

## 1. Objetivo

Idempotência é a propriedade que permite que uma mesma operação seja executada mais de uma vez sem produzir efeitos de negócio duplicados.

Em sistemas distribuídos, uma mensagem pode ser entregue novamente por diferentes motivos, como falhas de comunicação, retries ou redelivery. Por isso, o consumidor não deve assumir que uma mensagem será processada exatamente uma vez.

No OrderFlow, a idempotência é tratada em dois níveis:

- **Idempotência de mensagem:** utiliza o `EventId` e o padrão Inbox para impedir que o mesmo evento seja processado novamente.
- **Idempotência de negócio:** utiliza o `OrderId` para impedir que mais de um `Payment` seja registrado para o mesmo pedido.

Portanto:

```text
EventId → identifica uma entrega/evento
OrderId → identifica a operação de negócio
```
## 2. Idempotência de mensagem vs. idempotência de negócio

O Inbox implementado no OrderFlow protege o consumidor contra o processamento repetido de uma mesma mensagem.

Essa proteção utiliza o `EventId` como identificador único:

```text
EventId A
    ↓
Inbox
    ↓
processado uma única vez
```
Essa proteção resolve a duplicidade da mensagem, mas não necessariamente a duplicidade do efeito de negócio.

Por exemplo, duas mensagens diferentes podem possuir `EventId` distintos e ainda representar o mesmo pedido:

```text
EventId A ──┐
            ├── OrderId X → Payment
EventId B ──┘
```

Para o Inbox, essas são duas mensagens diferentes e ambas podem ser processadas.

Para o negócio, entretanto, ambas podem representar a tentativa de produzir o mesmo efeito: registrar um pagamento para o mesmo pedido.

Por isso, o OrderFlow utiliza identificadores diferentes para proteger níveis diferentes de idempotência:

| Nível | Identificador | Proteção |
|---|---|---|
| Mensagem | `EventId` | Evita o reprocessamento da mesma mensagem |
| Negócio | `OrderId` | Evita mais de um `Payment` para o mesmo pedido |

Os dois mecanismos são complementares. O Inbox protege o processamento da mensagem, enquanto a idempotência por `OrderId` protege o efeito produzido no domínio.

## 3. Idempotência no processamento de pagamentos

O processamento do pagamento é realizado pelo `ProcessPaymentCommandHandler`.

Antes de criar um novo `Payment`, a Application verifica se já existe um pagamento associado ao `OrderId` recebido:

```text
OrderCreatedMessage
        ↓
ProcessPaymentCommand
        ↓
Existe Payment para OrderId?
        ↓
   ┌────┴────┐
   │         │
  Sim       Não
   │         │
 return   cria Payment
             │
             ↓
        SaveChanges
```

Quando o pagamento já existe, o processamento simplesmente retorna sem criar um novo registro.

Conceitualmente:

```csharp
bool paymentExists =
    await _paymentRepository.ExistsByOrderIdAsync(
        request.OrderId,
        cancellationToken);

if (paymentExists)
    return;
```

Essa verificação torna o processamento idempotente no cenário sequencial.

Por exemplo:

```text
Evento A → OrderId X → Payment não existe → cria Payment
Evento B → OrderId X → Payment já existe → não cria outro
```

Mesmo que os eventos A e B possuam `EventId` diferentes, o segundo processamento não produz um novo efeito de negócio.

Entretanto, essa verificação isoladamente não é suficiente em um cenário concorrente, pois dois Workers podem consultar a existência do pagamento antes que qualquer um deles tenha realizado o `INSERT`.

Esse cenário será tratado na próxima seção.

## 4. Concorrência e condição de corrida

A verificação de existência realizada pela Application protege contra duplicidades sequenciais, mas não garante sozinha a idempotência quando múltiplos Workers processam eventos simultaneamente.

Considere dois eventos diferentes relacionados ao mesmo pedido:

```text
EventId A → OrderId X
EventId B → OrderId X
```

Se dois Workers processarem esses eventos ao mesmo tempo, ambos podem consultar o banco antes que qualquer pagamento tenha sido persistido:

```text
Worker A                         Worker B
   │                               │
   ├─ Payment existe? → Não        │
   │                               ├─ Payment existe? → Não
   │                               │
   ├─ cria Payment                 ├─ cria Payment
   │                               │
   ├─ INSERT                       ├─ INSERT
   │                               │
   └─ sucesso                      └─ ?
```

Esse problema é conhecido como uma condição de corrida do tipo **check-then-act**.

A sequência:

```text
verificar → decidir → inserir
```

não constitui uma operação atômica.

Portanto, mesmo existindo a verificação:

```csharp
if (paymentExists)
    return;
```

dois Workers ainda podem chegar simultaneamente à decisão de criar o pagamento.

Por esse motivo, a regra:

> Um pedido pode possuir apenas um pagamento registrado.

também precisa ser garantida estruturalmente pelo banco de dados.

No OrderFlow, essa garantia é realizada por um índice único sobre `Payments.OrderId`:

```csharp
builder.HasIndex(x => x.OrderId)
    .IsUnique()
    .HasDatabaseName("UX_Payments_OrderId");
```

Assim, mesmo que dois Workers ultrapassem simultaneamente a verificação da Application, o banco permite que apenas um deles persista o pagamento.

```text
Worker A                         Worker B
   │                               │
Payment não existe              Payment não existe
   │                               │
INSERT                          INSERT
   │                               │
sucesso                         conflito UNIQUE
```

A restrição UNIQUE funciona, portanto, como a garantia estrutural final da regra de idempotência de negócio.

## 5. Tratamento do conflito de unicidade

Quando dois Workers ultrapassam simultaneamente a verificação de existência, um deles consegue inserir o `Payment` e o outro encontra a restrição UNIQUE do banco de dados.

No SQL Server, esse tipo de conflito pode ser representado pelos códigos:

```text
2601 → chave duplicada em índice UNIQUE
2627 → violação de PRIMARY KEY ou UNIQUE constraint
```

Esses códigos são detalhes específicos da tecnologia de persistência e não devem ser conhecidos pela camada Application.

Por isso, o `UnitOfWork`, localizado na Infrastructure, traduz o `DbUpdateException` provocado por esses códigos para uma exceção independente do SQL Server:

```text
SqlException 2601/2627
        ↓
DbUpdateException
        ↓
Infrastructure
        ↓
UniqueConstraintException
        ↓
Application
```

Durante essa tradução, as entidades envolvidas na operação que falhou são removidas do `ChangeTracker`:

```csharp
foreach (var entry in exception.Entries)
{
    entry.State = EntityState.Detached;
}
```

Esse passo é necessário porque uma entidade cujo `INSERT` falhou permanece rastreada pelo EF Core. Sem o `Detach`, um `SaveChanges` posterior utilizando o mesmo `DbContext` poderia tentar inserir novamente a entidade conflitante.

Após receber a `UniqueConstraintException`, o `ProcessPaymentCommandHandler` consulta novamente a existência do pagamento:

```csharp
catch (UniqueConstraintException)
{
    bool paymentExistsAfterConflict =
        await _paymentRepository.ExistsByOrderIdAsync(
            request.OrderId,
            cancellationToken);

    if (paymentExistsAfterConflict)
        return;

    throw;
}
```

Essa segunda consulta diferencia um conflito concorrente esperado de uma falha que não pode ser considerada idempotente.

O fluxo completo fica:

```text
Worker A                         Worker B
   │                               │
Exists → false                  Exists → false
   │                               │
INSERT                          INSERT
   │                               │
sucesso                         UNIQUE 2601/2627
                                   │
                                   ↓
                         UnitOfWork traduz
                                   │
                                   ↓
                         remove entry do tracking
                                   │
                                   ↓
                         UniqueConstraintException
                                   │
                                   ↓
                         consulta novamente
                                   │
                                   ↓
                         Payment existe
                                   │
                                   ↓
                              return
```

Dessa forma, a violação da restrição UNIQUE causada pela concorrência deixa de ser tratada como uma falha de processamento e passa a representar um resultado idempotente quando o pagamento esperado já existe.

## 6. Integração com Inbox e RabbitMQ

A idempotência de negócio faz parte do processamento executado dentro do fluxo protegido pelo Inbox.

Quando uma mensagem `OrderCreatedMessage` é recebida, o Inbox registra o `EventId` e controla o estado do processamento. Durante esse processamento, o `ProcessPaymentCommandHandler` aplica a regra de idempotência utilizando o `OrderId`.

Assim, os dois mecanismos atuam em conjunto:

```text
RabbitMQ
   ↓
OrderCreatedMessage
   ↓
Inbox
   │
   ├─ EventId → controla a mensagem
   │
   ↓
ProcessPaymentCommand
   │
   ├─ OrderId → controla o efeito de negócio
   │
   ↓
Payment
```

Quando ocorre uma duplicidade concorrente de negócio, o banco pode rejeitar um dos `INSERTs` por meio da restrição UNIQUE.

Depois que a Application confirma que o `Payment` já foi criado pelo processamento concorrente, a operação é considerada concluída com sucesso.

Consequentemente, o fluxo pode terminar normalmente:

```text
conflito UNIQUE
      ↓
Payment já existe
      ↓
resultado idempotente
      ↓
Inbox → PROCESSED
      ↓
RabbitMQ → ACK
```

Nesse caso, não há necessidade de enviar a mensagem para retry, pois não existe uma falha transitória a ser corrigida por uma nova tentativa.

Isso é diferente de uma falha real:

```text
falha inesperada
      ↓
Inbox → FAILED
      ↓
Retry
      ↓
nova tentativa
```

Portanto, a idempotência também evita retries desnecessários quando múltiplos consumidores produzem concorrentemente o mesmo efeito de negócio.

## 7. Cenários validados

A implementação foi validada em diferentes cenários para verificar tanto a idempotência sequencial quanto a concorrente.

### 7.1 Processamento normal

Um evento relacionado a um `OrderId` ainda sem pagamento cria normalmente um novo `Payment`.

```text
EventId A
   ↓
OrderId X
   ↓
Payment não existe
   ↓
cria Payment
   ↓
PROCESSED
   ↓
ACK
```

### 7.2 Eventos diferentes para o mesmo pedido

Dois eventos com `EventId` diferentes foram processados para o mesmo `OrderId`.

O primeiro criou o pagamento e o segundo encontrou o pagamento existente.

```text
EventId A → OrderId X → cria Payment
EventId B → OrderId X → Payment já existe → return
```

O resultado final foi apenas um `Payment` para o pedido.

### 7.3 Processamento concorrente

O cenário concorrente foi validado utilizando duas instâncias do `Worker.Payments`.

Foi introduzido temporariamente um atraso no processamento para forçar os dois Workers a executarem a verificação de existência antes da persistência.

Os dois Workers observaram inicialmente:

```text
Payment existe? → Não
```

Em seguida, ambos tentaram criar um pagamento para o mesmo `OrderId`.

Um dos `INSERTs` foi persistido com sucesso e o outro foi rejeitado pelo índice único `UX_Payments_OrderId`.

O SQL Server retornou o erro `2601`, confirmando que a proteção estrutural do banco atuou durante a condição de corrida.

Após o tratamento da violação de unicidade, o segundo processamento confirmou que o pagamento já existia e terminou como sucesso idempotente.

O resultado observado foi:

```text
1 Payment para o OrderId
2 InboxMessages → PROCESSED
2 mensagens → ACK
0 retries causados pela duplicidade concorrente
```

O atraso artificial utilizado para provocar a condição de corrida foi removido após a conclusão do teste.

## 8. Responsabilidades por camada

A implementação mantém separadas as responsabilidades relacionadas à idempotência.

### Domain

O Domain contém o conceito de `Payment` e a abstração necessária para sua persistência:

```text
Payment
IPaymentRepository
```

O Domain não conhece EF Core, SQL Server, RabbitMQ ou códigos específicos de erro do banco.

### Application

A Application orquestra a regra de idempotência de negócio por meio do `ProcessPaymentCommandHandler`.

Suas responsabilidades são:

```text
verificar Payment por OrderId
        ↓
criar Payment quando necessário
        ↓
solicitar persistência
        ↓
interpretar conflito de unicidade
        ↓
confirmar se o efeito já existe
```

A Application conhece `UniqueConstraintException`, mas não conhece os códigos `2601` ou `2627` do SQL Server.

### Infrastructure

A Infrastructure implementa os mecanismos técnicos necessários para garantir a regra:

```text
PaymentRepository
        +
índice UNIQUE em OrderId
        +
UnitOfWork
        +
tradução 2601/2627
        +
controle do ChangeTracker
```

É nessa camada que os detalhes específicos do EF Core e do SQL Server permanecem encapsulados.

### Worker.Payments

O Worker atua como ponto de entrada da mensagem.

Seu papel é receber o `OrderCreatedMessage` e delegar o processamento:

```text
RabbitMQ
   ↓
OrderCreatedConsumer
   ↓
InboxProcessor
   ↓
ProcessPaymentCommand
   ↓
Application
```

O consumidor não implementa diretamente a regra de idempotência do pagamento.

Essa separação mantém a regra de negócio independente do mecanismo de transporte e concentra os detalhes de persistência na Infrastructure.

## 9. Conclusão

Idempotência em sistemas distribuídos não deve depender apenas da identificação da mensagem.

No OrderFlow, a solução foi construída em duas camadas complementares:

```text
EventId
   ↓
Inbox
   ↓
Idempotência de mensagem

OrderId
   ↓
Payment
   ↓
Idempotência de negócio
```

O Inbox impede que a mesma mensagem seja processada novamente, enquanto a regra baseada em `OrderId` impede que eventos diferentes produzam pagamentos duplicados para o mesmo pedido.

A verificação realizada pela Application reduz tentativas desnecessárias de persistência, mas não é suficiente para garantir a regra em cenários concorrentes.

Por isso, a solução combina:

```text
verificação na Application
        +
restrição UNIQUE no banco
        +
tradução da violação de unicidade
        +
confirmação do efeito de negócio
```

Essa combinação permite que o sistema mantenha apenas um `Payment` por `OrderId`, inclusive quando múltiplos Workers processam eventos concorrentes.

A principal conclusão é:

> Idempotência não significa impedir que uma operação seja tentada novamente. Significa garantir que múltiplas tentativas não produzam efeitos de negócio duplicados.

A restrição do banco fornece a garantia estrutural final, enquanto a Application interpreta corretamente o conflito concorrente como sucesso idempotente quando o efeito esperado já existe.