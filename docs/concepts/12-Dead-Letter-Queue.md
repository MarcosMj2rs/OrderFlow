# Dead Letter Queue

**Status:** Aceita

## 1. O que é uma Dead Letter Queue?

Uma **Dead Letter Queue (DLQ)** é uma fila destinada a armazenar mensagens que não puderam continuar no fluxo normal de processamento.

Em vez de descartar a mensagem ou mantê-la retornando indefinidamente para a fila principal, o sistema a isola em uma fila específica para posterior investigação.

No OrderFlow, uma mensagem pode chegar à DLQ principalmente em dois cenários:

- quando ocorre uma falha permanente, para a qual novas tentativas não possuem expectativa de sucesso;
- quando uma falha transitória continua ocorrendo e a mensagem excede o limite máximo de Retry.

A DLQ funciona, portanto, como um mecanismo de **isolamento de falhas**.

```text
Mensagem
    |
    v
Consumer
    |
    +---- Sucesso ----------------------> ACK
    |
    +---- Falha transitória ------------> Retry
    |
    +---- Falha permanente -------------> DLQ
    |
    +---- Limite de Retry excedido -----> DLQ
```

Uma mensagem armazenada na DLQ deixa de interferir no processamento das demais mensagens, mas continua disponível para investigação e eventual reprocessamento posterior.

---

## 2. Por que uma DLQ é necessária?

Em sistemas orientados a mensagens, nem toda falha pode ser resolvida executando novamente o processamento.

Sem uma DLQ, uma mensagem problemática poderia permanecer sendo entregue repetidamente ao consumer, provocando:

- ciclos contínuos de processamento;
- consumo desnecessário de recursos;
- aumento de logs de erro;
- interferência no processamento de outras mensagens;
- dificuldade de diagnóstico;
- risco de descarte e perda da mensagem.

Também não é adequado executar Retry indefinidamente.

O Retry deve possuir um limite. Quando esse limite é excedido, a mensagem precisa sair do fluxo normal de processamento.

A DLQ fornece esse destino seguro:

```text
Main Queue
    |
    v
Consumer
    |
    v
Falha transitória
    |
    v
Retry
    |
    v
Limite excedido
    |
    v
DLQ
```

Para falhas classificadas como permanentes, o Retry não agrega valor:

```text
Main Queue
    |
    v
Consumer
    |
    v
Falha permanente
    |
    v
DLQ
```

Dessa forma, Retry e DLQ possuem responsabilidades diferentes:

- **Retry** tenta recuperar mensagens de falhas potencialmente temporárias;
- **DLQ** isola mensagens que não devem continuar no fluxo normal.

Esses mecanismos se complementam para aumentar a confiabilidade do processamento assíncrono.

---

## 3. Falhas transitórias e falhas permanentes

A decisão de utilizar Retry ou enviar uma mensagem para a DLQ depende da natureza da falha.

### Falha transitória

Uma falha transitória é aquela que pode desaparecer sem alteração da mensagem.

Exemplos:

- indisponibilidade temporária de um serviço;
- timeout;
- falha temporária de comunicação;
- recurso momentaneamente indisponível.

Nesse caso, a mensagem segue para o fluxo de Retry.

```text
Falha transitória
      |
      v
    Retry
      |
      +---- Nova tentativa com sucesso ----> ACK
      |
      +---- Limite excedido ---------------> DLQ
```

### Falha permanente

Uma falha permanente é aquela para a qual repetir o mesmo processamento não possui expectativa de resolver o problema.

Exemplos:

- payload inválido;
- identificador obrigatório inválido;
- contrato incompatível;
- inconsistência permanente de dados.

Nesse caso, executar Retry apenas consumiria recursos e atrasaria o isolamento da mensagem.

Por isso, no OrderFlow, falhas permanentes são publicadas diretamente na DLQ:

```text
Falha permanente
       |
       v
      DLQ
```

No `OrderCreatedConsumer`, por exemplo, identificadores obrigatórios vazios são classificados como falhas permanentes.

Essa separação evita tratar todas as falhas da mesma forma e permite aplicar a estratégia adequada para cada tipo de erro.

---

## 4. Topologia da DLQ no OrderFlow

Para o processamento do evento `order.created`, o OrderFlow utiliza três filas com responsabilidades distintas:

```text
orderflow.order-created
        |
        | falha transitória
        v
orderflow.order-created.retry
        |
        | Expiration
        v
orderflow.order-created

orderflow.order-created
        |
        | falha permanente
        | ou limite de Retry excedido
        v
orderflow.order-created.dlq
```

### Main Queue

```text
orderflow.order-created
```

É a fila principal consumida pelo `OrderCreatedConsumer`.

### Retry Queue

```text
orderflow.order-created.retry
```

Armazena temporariamente mensagens que apresentaram falhas transitórias.

Após o período definido pela propriedade `Expiration` da mensagem, o RabbitMQ realiza o dead lettering da mensagem de Retry de volta para o fluxo de `order.created`.

### Dead Letter Queue

```text
orderflow.order-created.dlq
```

Armazena mensagens que não devem continuar no fluxo normal de processamento.

A DLQ está vinculada à exchange do OrderFlow utilizando a routing key:

```text
order.created.dlq
```

Diferentemente da Retry Queue, a DLQ não possui configuração para devolver automaticamente a mensagem à fila principal.

As mensagens permanecem isoladas até que exista uma ação operacional para analisá-las e, futuramente, reprocessá-las ou removê-las.

---

## 5. Publicação para a DLQ e ACK da mensagem original

No OrderFlow, o envio para a DLQ é realizado por meio de uma nova publicação.

Quando o `RabbitMqConsumerBase` determina que uma mensagem deve seguir para a DLQ, o fluxo é:

```text
Mensagem original
      |
      v
Consumer
      |
      v
Falha permanente
ou limite de Retry
      |
      v
Publicação com
order.created.dlq
      |
      v
Dead Letter Queue
      |
      v
ACK da entrega original
```

A ordem dessas operações é importante.

O `ACK` da entrega original somente ocorre depois que a publicação destinada à DLQ é concluída com sucesso.

Conceitualmente:

```text
Publicar na DLQ
      |
      v
Publicação concluída
      |
      v
ACK da mensagem original
```

Se a publicação falhar antes de sua conclusão, o fluxo não deve executar o `ACK` da entrega original.

Essa estratégia reduz o risco de ocorrer a seguinte situação:

```text
ACK da original
      |
      v
Falha ao publicar na DLQ
      |
      v
Mensagem perdida
```

Por isso, o OrderFlow utiliza um canal de publicação com publisher confirmations habilitado e mantém a publicação antes do `ACK` da entrega original.

Essa abordagem também é compatível com a semântica **At Least Once** adotada pelo sistema.

---

## 6. Preservação da mensagem e dos metadados

Uma mensagem enviada para a DLQ deve manter informações suficientes para permitir sua investigação.

No OrderFlow, a republicação preserva o `Body` original e copia propriedades AMQP relevantes da entrega recebida.

Entre elas:

- Headers;
- Delivery Mode;
- MessageId;
- CorrelationId;
- ContentType;
- ContentEncoding;
- Type;
- Timestamp;
- AppId.

O header utilizado para controlar as tentativas também é mantido:

```text
x-orderflow-retry-count
```

Para uma falha permanente que não passou pelo Retry, por exemplo:

```text
x-orderflow-retry-count = 0
```

Para uma mensagem que esgotou três tentativas de Retry:

```text
x-orderflow-retry-count = 3
```

A routing key original, por outro lado, não precisa ser preservada como routing key da nova publicação.

Ao enviar a mensagem para a DLQ, o OrderFlow utiliza:

```text
order.created.dlq
```

Isso é necessário para que a exchange encaminhe a nova publicação para:

```text
orderflow.order-created.dlq
```

Essa preservação permite correlacionar a mensagem, analisar seu conteúdo e compreender o histórico de tentativas que antecedeu seu isolamento.

Durante os testes do capítulo, foi possível confirmar a preservação do payload, headers, `Delivery Mode`, `MessageId` e `CorrelationId` após a publicação na DLQ.

---

## 7. Isolamento e reprocessamento

Uma mensagem publicada na DLQ deixa de participar automaticamente do fluxo normal de processamento.

No OrderFlow, a fila:

```text
orderflow.order-created.dlq
```

não possui um mecanismo automático para devolver suas mensagens à fila principal.

Portanto:

```text
Main Queue
    |
    v
Falha
    |
    v
DLQ
    |
    X
Retorno automático
```

Esse comportamento é intencional.

Uma mensagem que chegou à DLQ pode representar um problema que precisa ser compreendido antes de qualquer nova tentativa, como:

- payload inválido;
- dados inconsistentes;
- contrato incompatível;
- falha permanente de validação;
- falha transitória que persistiu além do limite de Retry.

Reprocessar automaticamente essas mensagens poderia simplesmente repetir o problema e devolver instabilidade ao fluxo principal.

Por isso, nesta etapa do OrderFlow, o reprocessamento será iniciado manualmente somente após a investigação da causa da falha.

O procedimento operacional para realizar esse reprocessamento ainda não faz parte da implementação atual e será definido como evolução posterior.

Durante os testes do capítulo, uma mensagem permaneceu na DLQ sem retornar para a fila principal ou para a Retry Queue, confirmando o isolamento esperado.

---

## 8. Relação com Retry, Inbox e Idempotência

A DLQ não funciona isoladamente. Ela faz parte da estratégia de confiabilidade do processamento assíncrono do OrderFlow.

### Retry

O Retry trata falhas consideradas transitórias.

```text
Falha transitória
      |
      v
    Retry
      |
      +---- Sucesso ----------> ACK
      |
      +---- Limite excedido --> DLQ
```

A DLQ estabelece o destino final da mensagem quando novas tentativas deixam de ser úteis.

Falhas permanentes não precisam percorrer esse caminho e podem seguir diretamente para a DLQ.

### Inbox

Como o OrderFlow utiliza entrega **At Least Once**, uma mensagem pode ser entregue mais de uma vez.

O Inbox Pattern controla o processamento das mensagens utilizando o `EventId`, reduzindo o risco de executar novamente o mesmo processamento devido a uma redelivery.

```text
Mensagem
   |
   v
Inbox
   |
   +---- já processada ----> não repete processamento
   |
   +---- nova -------------> processa
```

### Idempotência

A idempotência protege o efeito de negócio mesmo quando diferentes entregas ou eventos tentam produzir o mesmo resultado.

No fluxo de pagamento do OrderFlow, por exemplo, a unicidade por `OrderId` impede a criação de mais de um pagamento para o mesmo pedido.

Essa proteção será especialmente importante quando o reprocessamento de mensagens da DLQ for implementado.

Portanto, os mecanismos possuem responsabilidades complementares:

| Mecanismo | Responsabilidade |
|---|---|
| Retry | Recuperar falhas transitórias |
| DLQ | Isolar mensagens que não devem continuar no fluxo normal |
| Inbox | Controlar o processamento duplicado da mesma mensagem |
| Idempotência | Proteger o efeito de negócio contra duplicidade |

Em conjunto, esses mecanismos tornam o processamento assíncrono mais resiliente sem depender da hipótese de que cada mensagem será entregue e processada exatamente uma vez.

---

## 9. Cenários validados no OrderFlow

A implementação da Dead Letter Queue foi validada com diferentes cenários.

### Falha permanente

Uma mensagem com `EventId` vazio foi classificada como falha permanente.

Resultado:

```text
Mensagem original
      |
      v
PermanentMessagingException
      |
      v
RetryCount = 0
      |
      v
DLQ
```

A mensagem foi publicada diretamente na DLQ sem passar pela Retry Queue.

### Limite de Retry excedido

Uma falha transitória controlada foi utilizada para validar o esgotamento das tentativas.

O fluxo observado foi:

```text
Tentativa original
      |
      v
Retry 1 - 10000 ms
      |
      v
Retry 2 - 20000 ms
      |
      v
Retry 3 - 40000 ms
      |
      v
Limite excedido
      |
      v
DLQ
```

Após o terceiro Retry, uma nova falha resultou no encaminhamento da mensagem para a DLQ.

### Preservação da mensagem

Também foi validada a preservação de:

- payload;
- headers;
- Delivery Mode;
- MessageId;
- CorrelationId;
- Retry Count.

A routing key da nova publicação foi corretamente alterada para:

```text
order.created.dlq
```

### Isolamento

A mensagem armazenada na DLQ permaneceu isolada, sem retornar automaticamente para:

```text
orderflow.order-created
```

ou:

```text
orderflow.order-created.retry
```

Esses testes demonstram o comportamento esperado da estratégia de DLQ adotada pelo OrderFlow.

---

## 10. Resumo

A Dead Letter Queue é o destino das mensagens que não devem continuar no fluxo normal de processamento.

No OrderFlow:

- falhas permanentes são publicadas diretamente na DLQ;
- falhas transitórias utilizam Retry antes da DLQ;
- mensagens que excedem o limite máximo de Retry são publicadas na DLQ;
- a mensagem original é confirmada somente após a publicação destinada à DLQ;
- o body e os metadados AMQP relevantes são preservados;
- o Retry Count acompanha a mensagem;
- mensagens armazenadas na DLQ não retornam automaticamente ao fluxo principal;
- o reprocessamento automático não faz parte da estratégia atual;
- eventual reprocessamento deverá ser iniciado manualmente após investigação.

O fluxo completo pode ser resumido como:

```text
                    +-------------------+
                    |    Main Queue     |
                    +---------+---------+
                              |
                              v
                    +-------------------+
                    |     Consumer      |
                    +---------+---------+
                              |
              +---------------+---------------+
              |                               |
              v                               v
     Falha transitória               Falha permanente
              |                               |
              v                               |
        +-----------+                         |
        |   Retry   |                         |
        +-----+-----+                         |
              |                               |
       +------+-------+                       |
       |              |                       |
       v              v                       |
    Sucesso     Limite excedido               |
       |              |                       |
       v              +-----------+-----------+
      ACK                         |
                                  v
                         +----------------+
                         |      DLQ       |
                         +----------------+
```

Retry tenta recuperar.

DLQ isola o que não pôde ser recuperado.

Inbox controla duplicidade de mensagens.

Idempotência protege os efeitos de negócio.

Esses mecanismos, utilizados em conjunto, formam parte da estratégia de **Reliable Messaging** do OrderFlow.