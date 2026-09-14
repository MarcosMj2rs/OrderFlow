# DEC-013 — Por que utilizar Dead Letter Queue?

**Status:** Aceita

## Contexto

Em um sistema orientado a mensagens, algumas mensagens podem falhar de maneira permanente ou continuar apresentando falhas mesmo após sucessivas tentativas de Retry.

Manter essas mensagens indefinidamente no fluxo normal de processamento pode provocar:

- novas tentativas sem expectativa de sucesso;
- consumo desnecessário de recursos;
- aumento de logs de erro;
- interferência no processamento de outras mensagens;
- dificuldade para investigar a causa da falha.

Descartar essas mensagens também não é uma alternativa adequada, pois elimina informações importantes para diagnóstico e impede um eventual reprocessamento posterior.

O OrderFlow precisa, portanto, de um mecanismo que permita retirar essas mensagens do fluxo normal sem descartá-las.

---

## Decisão

Utilizar **Dead Letter Queue (DLQ)** para isolar mensagens que não devem continuar no fluxo normal de processamento.

Uma mensagem será encaminhada para a DLQ quando:

- ocorrer uma falha classificada como permanente;
- uma falha transitória continuar ocorrendo após o limite máximo de Retry.

O fluxo conceitual será:

```text
Mensagem
    |
    v
Consumer
    |
    +---- Sucesso ----------------------> ACK
    |
    +---- Falha transitória ------------> Retry
    |                                      |
    |                                      v
    |                              Limite excedido
    |                                      |
    |                                      v
    |                                     DLQ
    |
    +---- Falha permanente -------------> DLQ
```

No fluxo atualmente implementado para `order.created`, a DLQ utilizada é:

```text
orderflow.order-created.dlq
```

com a routing key:

```text
order.created.dlq
```

---

## Por que utilizar uma DLQ?

A DLQ permite separar mensagens problemáticas das mensagens que ainda podem ser processadas normalmente.

Isso evita que uma mensagem permaneça indefinidamente no fluxo principal e permite preservar seu conteúdo para investigação.

A alternativa de simplesmente descartar a mensagem não foi adotada porque dificultaria:

- identificar a causa da falha;
- analisar o payload recebido;
- correlacionar a mensagem com outras operações;
- compreender o histórico de tentativas;
- realizar eventual reprocessamento.

A DLQ funciona, portanto, como um mecanismo de **isolamento de falhas**, e não como um mecanismo de recuperação automática.

---

## Relação com Retry

Retry e DLQ possuem responsabilidades diferentes.

O **Retry** é utilizado quando existe expectativa de que uma nova tentativa possa resolver uma falha transitória.

A **DLQ** é utilizada quando a mensagem não deve continuar no fluxo normal.

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

Para falhas permanentes:

```text
Falha permanente
       |
       v
      DLQ
```

Dessa forma, falhas permanentes não consomem tentativas de Retry desnecessariamente.

---

## Estratégia de publicação

No OrderFlow, o encaminhamento para a DLQ é realizado por meio de uma nova publicação utilizando a routing key destinada à DLQ.

O fluxo é:

```text
Mensagem original
       |
       v
Consumer
       |
       v
Publicação para DLQ
       |
       v
Publicação concluída
       |
       v
ACK da entrega original
```

A ordem é importante.

O `ACK` da entrega original ocorre somente depois que a publicação destinada à DLQ é concluída.

O canal utilizado para essa republicação possui **publisher confirmations** habilitado.

Essa estratégia reduz o risco de confirmar a mensagem original antes da conclusão da nova publicação e é compatível com a semântica **At Least Once** utilizada pelo OrderFlow.

---

## Preservação da mensagem

A republicação para a DLQ deve preservar informações relevantes para investigação.

No OrderFlow são preservados o `Body` original e propriedades AMQP relevantes, incluindo:

- Headers;
- Delivery Mode;
- MessageId;
- CorrelationId;
- ContentType;
- ContentEncoding;
- Type;
- Timestamp;
- AppId.

O header:

```text
x-orderflow-retry-count
```

também acompanha a mensagem, permitindo identificar quantas tentativas ocorreram antes do encaminhamento para a DLQ.

A routing key da nova publicação é alterada para a routing key correspondente à DLQ.

Para `order.created`:

```text
order.created.dlq
```

---

## Reprocessamento

Mensagens armazenadas na DLQ **não serão reprocessadas automaticamente**.

Essa decisão evita que mensagens já classificadas como problemáticas retornem ao fluxo principal sem que a causa da falha tenha sido investigada.

O fluxo adotado é:

```text
DLQ
 |
 v
Investigação
 |
 v
Correção da causa
 |
 v
Eventual reprocessamento manual
```

O procedimento operacional para reprocessamento manual não faz parte da implementação atual e poderá ser definido posteriormente.

---

## Relação com Inbox e Idempotência

A utilização de DLQ não elimina a possibilidade de uma mensagem ser entregue novamente.

O OrderFlow trabalha com semântica **At Least Once**, portanto duplicidades precisam continuar sendo consideradas.

O Inbox Pattern protege o processamento da mesma mensagem utilizando o `EventId`.

A idempotência de negócio protege os efeitos produzidos pelo processamento. No fluxo de pagamento, por exemplo, a unicidade por `OrderId` impede a criação de mais de um pagamento para o mesmo pedido.

Essas proteções serão especialmente importantes quando mensagens da DLQ forem eventualmente reprocessadas.

```text
Retry
  |
  | recupera falhas transitórias
  v

DLQ
  |
  | isola mensagens problemáticas
  v

Inbox
  |
  | controla duplicidade da mensagem
  v

Idempotência
  |
  | protege o efeito de negócio
  v
```

---

## Alternativas consideradas

### Retry indefinido

**Não adotado.**

Uma mensagem com falha permanente poderia permanecer sendo processada indefinidamente.

Isso consumiria recursos sem expectativa de recuperação.

### Descartar a mensagem

**Não adotado.**

O descarte impediria investigação posterior e poderia provocar perda de informações importantes para diagnóstico.

### Reprocessamento automático da DLQ

**Não adotado nesta etapa.**

Uma mensagem presente na DLQ pode exigir investigação ou correção antes de uma nova tentativa.

Reprocessá-la automaticamente poderia simplesmente reproduzir a mesma falha.

### Dead Letter Queue

**Adotado.**

Permite retirar a mensagem problemática do fluxo normal, preservá-la para investigação e manter aberta a possibilidade de reprocessamento posterior.

---

## Consequências positivas

A adoção da DLQ proporciona:

- isolamento de mensagens problemáticas;
- proteção do fluxo normal de processamento;
- eliminação de Retry indefinido;
- preservação das mensagens para investigação;
- melhor capacidade de diagnóstico;
- possibilidade de reprocessamento posterior;
- separação clara entre recuperação por Retry e isolamento por DLQ.

---

## Consequências negativas

A utilização de DLQ também introduz responsabilidades adicionais:

- as filas precisam ser monitoradas;
- mensagens podem permanecer acumuladas;
- é necessário investigar as causas das falhas;
- será necessário definir um procedimento operacional para eventual reprocessamento;
- a DLQ passa a fazer parte da operação e manutenção do sistema.

Portanto, possuir uma DLQ não resolve a causa da falha. Ela apenas fornece um local seguro para isolar e investigar a mensagem.

---

## Implementação atual

Para o evento `order.created`, a topologia utilizada é:

```text
orderflow.order-created
        |
        +---- falha transitória
        |          |
        |          v
        |  orderflow.order-created.retry
        |          |
        |          | Expiration
        |          v
        +---- orderflow.order-created
        |
        +---- falha permanente ------------------+
        |                                         |
        +---- limite máximo de Retry excedido ----+
                                                  |
                                                  v
                                  orderflow.order-created.dlq
```

O `RabbitMqConsumerBase` centraliza a infraestrutura necessária para:

- identificar o tratamento de falhas permanentes e transitórias;
- controlar o Retry Count;
- publicar mensagens para Retry;
- publicar mensagens para DLQ;
- preservar o body e metadados relevantes;
- realizar o `ACK` da entrega original após a publicação correspondente.

O consumer concreto permanece responsável pelo processamento específico da mensagem e pela classificação das situações de negócio que devem resultar em falha permanente ou transitória.

---

## Validação

A estratégia foi validada no OrderFlow com os seguintes cenários:

1. falha permanente enviada diretamente para a DLQ, sem Retry;
2. falha transitória passando pelas tentativas de Retry até atingir o limite e seguir para a DLQ;
3. preservação do payload, headers, `Delivery Mode`, `MessageId` e `CorrelationId`;
4. preservação do `x-orderflow-retry-count`;
5. alteração da routing key para `order.created.dlq`;
6. permanência da mensagem na DLQ sem retorno automático para a fila principal ou Retry Queue.

A implementação também mantém a publicação destinada à DLQ antes do `ACK` da entrega original.

---

## Resultado

A **Dead Letter Queue foi adotada** como parte da estratégia de Reliable Messaging do OrderFlow.

Ela fornece um destino controlado para mensagens que não podem continuar no fluxo normal, preservando-as para investigação sem comprometer o processamento das demais mensagens.

A estratégia final combina:

```text
Falha transitória
       |
       v
     Retry
       |
       +---- recuperada -------> ACK
       |
       +---- não recuperada ---> DLQ

Falha permanente -------------> DLQ
```

A DLQ complementa Retry, Inbox e Idempotência, formando uma estratégia de processamento assíncrono compatível com **At Least Once**.