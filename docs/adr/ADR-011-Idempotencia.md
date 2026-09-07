# ADR-011 — Idempotência no processamento de pagamentos

## Status

Aceito

## Contexto

O `Worker.Payments` consome eventos `OrderCreatedMessage` e registra um `Payment` associado ao pedido recebido.

O padrão Inbox já garante idempotência no nível da mensagem utilizando o `EventId`. Entretanto, essa proteção não é suficiente para garantir idempotência no nível de negócio.

Dois eventos distintos podem possuir `EventId` diferentes e ainda representar o mesmo `OrderId`:

```text
EventId A ──┐
            ├── OrderId X → Payment
EventId B ──┘
```

Nesse cenário, ambos os eventos são válidos do ponto de vista do Inbox, mas poderiam produzir dois pagamentos para o mesmo pedido.

Além disso, uma simples verificação de existência antes da criação do pagamento não elimina condições de corrida. Dois Workers podem verificar simultaneamente que nenhum pagamento existe e tentar persistir o mesmo efeito de negócio.

A arquitetura precisa, portanto, garantir a seguinte regra:

> Um `OrderId` pode possuir apenas um `Payment` registrado.

Essa garantia deve permanecer válida inclusive durante processamento concorrente por múltiplos Workers.

## Decisão

A idempotência do processamento de pagamentos será garantida utilizando o `OrderId` como chave de negócio.

Antes de criar um `Payment`, a Application verifica se já existe um pagamento associado ao pedido:

```text
OrderId
   ↓
Payment existe?
   │
   ├─ Sim → encerra o processamento
   │
   └─ Não → cria Payment
```

Essa verificação será complementada por uma restrição de unicidade no banco de dados sobre `Payments.OrderId`.

```text
Payments
   ↓
UNIQUE(OrderId)
```

A restrição no banco será considerada a garantia estrutural final da regra, protegendo o sistema contra condições de corrida entre múltiplos Workers.

Quando uma tentativa concorrente resultar em violação de unicidade, a Infrastructure traduzirá os erros específicos do SQL Server `2601` e `2627` para `UniqueConstraintException`.

A Application não conhecerá detalhes específicos do SQL Server.

Após receber `UniqueConstraintException`, o processamento consultará novamente a existência do `Payment` para o `OrderId`.

Se o pagamento existir, o conflito será interpretado como sucesso idempotente. Caso contrário, a exceção continuará sendo propagada.

```text
UNIQUE violation
       ↓
Infrastructure
       ↓
UniqueConstraintException
       ↓
Application
       ↓
Payment existe?
   ┌───┴───┐
  Sim     Não
   │       │
 return   throw
```

As entidades envolvidas em uma operação de persistência rejeitada por violação de unicidade serão removidas do `ChangeTracker` antes da propagação de `UniqueConstraintException`, evitando novas tentativas involuntárias de persistência pelo mesmo `DbContext`.

## Consequências

### Positivas

A regra de um único pagamento por pedido passa a possuir uma garantia estrutural no banco de dados, não dependendo exclusivamente de verificações realizadas pela aplicação.

O processamento permanece idempotente mesmo quando dois Workers tentam registrar simultaneamente um `Payment` para o mesmo `OrderId`.

A Application permanece independente de detalhes específicos do SQL Server, pois os códigos `2601` e `2627` são interpretados pela Infrastructure.

Conflitos concorrentes esperados podem terminar como sucesso idempotente, evitando retries desnecessários quando o efeito de negócio já foi produzido.

A utilização conjunta de Inbox e idempotência de negócio fornece proteções complementares:

```text
EventId → duplicidade da mensagem
OrderId → duplicidade do efeito de negócio
```

### Negativas

A implementação adiciona tratamento específico para violações de unicidade na Infrastructure.

Uma falha de persistência por unicidade exige tratamento do estado das entidades no `ChangeTracker` do EF Core antes que o mesmo `DbContext` seja reutilizado.

A estratégia utiliza uma verificação inicial seguida de uma restrição UNIQUE. Portanto, em cenários concorrentes, ainda pode ocorrer uma tentativa de `INSERT` rejeitada pelo banco.

Esse conflito é esperado e faz parte da estratégia adotada; a restrição do banco é a responsável por resolver definitivamente a corrida.

## Alternativas consideradas

### Utilizar apenas o Inbox

Foi considerado utilizar somente o `EventId` registrado pelo Inbox para garantir a idempotência.

Essa alternativa foi rejeitada porque o Inbox identifica mensagens, não necessariamente operações de negócio.

Dois eventos com `EventId` diferentes podem possuir o mesmo `OrderId` e produzir o mesmo efeito de negócio.

### Utilizar apenas a verificação na Application

Também foi considerada somente a consulta de existência antes da criação do pagamento:

```text
Payment existe?
   ↓
Sim → return
Não → INSERT
```

Essa alternativa foi rejeitada como garantia única porque a sequência de verificação e inserção não é atômica.

Dois Workers podem verificar simultaneamente que o pagamento não existe e ambos tentarem realizar o `INSERT`.

### Utilizar somente a restrição UNIQUE

A utilização exclusiva da restrição UNIQUE protegeria a consistência dos dados, mas faria com que duplicidades sequenciais também chegassem desnecessariamente ao banco como tentativas de inserção inválidas.

Além disso, uma violação de unicidade seria inicialmente observada como uma falha técnica, mesmo quando representasse apenas a repetição de um efeito de negócio já realizado.

### Estratégia escolhida

Foi escolhida a combinação:

```text
Inbox por EventId
        +
verificação por OrderId
        +
UNIQUE(OrderId)
        +
tratamento do conflito concorrente
```

A verificação na Application otimiza o cenário comum, enquanto a restrição UNIQUE fornece a garantia necessária diante de concorrência.

O tratamento posterior do conflito permite distinguir uma duplicidade concorrente esperada de uma falha que deve continuar sendo propagada.

