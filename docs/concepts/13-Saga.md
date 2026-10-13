# 13 — Saga Pattern

## 1. Objetivo

O padrão Saga permite coordenar operações de negócio que envolvem múltiplas etapas, especialmente em sistemas distribuídos, nos quais não existe uma única transação de banco de dados abrangendo todos os serviços.

Uma Saga é composta por uma sequência de transações locais. Cada etapa persiste suas próprias alterações e, quando necessário, publica eventos para permitir a continuidade do processo.

Quando uma etapa falha definitivamente, a Saga pode executar ações compensatórias para desfazer logicamente os efeitos de operações anteriores.

O OrderFlow utiliza o padrão Saga para coordenar o processamento de pedidos, pagamentos e reservas de inventário.

## 2. Estratégia adotada: orquestração

Existem duas abordagens comuns para implementar uma Saga:

- **Coreografia:** os participantes reagem a eventos sem um coordenador central.
- **Orquestração:** um componente coordena explicitamente as etapas e as transições de estado.

O OrderFlow adota uma abordagem de **orquestração orientada a eventos**, utilizando MediatR, RabbitMQ e uma entidade persistente `OrderSaga`.

O estado da Saga é armazenado no SQL Server, permitindo que o processamento seja retomado após determinadas falhas.

## 3. Componentes da implementação

| Componente | Responsabilidade |
|---|---|
| `OrderSaga` | Representar o estado e as transições da Saga |
| `OrderSagaOrchestrator` | Iniciar a Saga e coordenar o processamento inicial |
| `StartOrderSagaCommand` | Solicitar o início da Saga |
| `HandlePaymentCompletedCommand` | Processar a conclusão do pagamento |
| `HandleInventoryReservedCommand` | Processar a confirmação da reserva |
| `ProcessPaymentCommand` | Processar o pagamento |
| `ReserveInventoryCommand` | Criar a reserva de inventário |
| `PayOrderCommand` | Alterar o pedido para `PAID` |
| `InboxProcessor` | Controlar o processamento e o reprocessamento de mensagens |
| `Outbox` | Persistir eventos para publicação confiável |
| `RabbitMQ` | Transportar os eventos entre as etapas |

## 4. Fluxo principal

O processamento bem-sucedido segue esta sequência:

```text
OrderCreatedMessage
        |
        v
StartOrderSagaCommand
        |
        v
Saga STARTED
        |
        v
ProcessPaymentCommand
        |
        v
Payment COMPLETED
        |
        v
PaymentCompletedMessage
        |
        v
HandlePaymentCompletedCommand
        |
        v
Saga PAYMENT_COMPLETED
        |
        v
ReserveInventoryCommand
        |
        v
InventoryReservation RESERVED
        |
        v
InventoryReservedMessage
        |
        v
HandleInventoryReservedCommand
        |
        v
Saga INVENTORY_RESERVED
        |
        v
PayOrderCommand
        |
        v
Order PAID
        |
        v
Saga COMPLETED
```

A comunicação utiliza eventos publicados pelo Outbox e consumidos pelos Workers por meio do RabbitMQ.

## 5. Estados da Saga

A entidade `OrderSaga` utiliza a enumeração `EOrderSagaStatus`.

| Estado | Valor | Significado |
|---|---:|---|
| `STARTED` | 1 | Saga iniciada |
| `PAYMENT_COMPLETED` | 2 | Pagamento concluído |
| `INVENTORY_RESERVED` | 3 | Reserva de inventário concluída |
| `COMPLETED` | 4 | Processamento concluído |
| `COMPENSATING` | 5 | Compensação em andamento |
| `COMPENSATED` | 6 | Compensação concluída |
| `FAILED` | 7 | Saga encerrada com falha |

Os estados de compensação estão previstos no domínio, mas sua integração ao fluxo de orquestração ainda não foi concluída.

## 6. Persistência e consistência eventual

As etapas da Saga não são executadas em uma única transação distribuída.

Cada comando persiste suas alterações por meio do `IUnitOfWork`.

Isso significa que o banco pode apresentar temporariamente estados intermediários, como:

- Pagamento concluído, mas inventário ainda não reservado.
- Inventário reservado, mas pedido ainda pendente.
- Pedido pago, mas Saga ainda não finalizada.

Essas situações são esperadas em uma arquitetura baseada em consistência eventual.

A implementação precisa permitir que operações interrompidas sejam retomadas sem repetir indevidamente seus efeitos de negócio.

## 7. Idempotência e recuperação

A recuperação utiliza a combinação de estados persistidos, handlers idempotentes e processamento controlado pelo Inbox.

### 7.1. Recuperação após `PAYMENT_COMPLETED`

O `HandlePaymentCompletedCommandHandler` verifica o estado atual da Saga.

Se estiver em `STARTED`, executa `MarkPaymentCompleted()` e persiste a transição.

Se já estiver em `PAYMENT_COMPLETED`, não repete a transição e prossegue para `ReserveInventoryCommand`.

O `ReserveInventoryCommandHandler` verifica se já existe uma reserva para o pedido.

Se existir, retorna sem criar outra reserva.

Também existe tratamento para conflitos de unicidade, com nova consulta após a exceção.

### 7.2. Recuperação após `INVENTORY_RESERVED`

O `HandleInventoryReservedCommandHandler` verifica o estado da Saga.

Se estiver em `PAYMENT_COMPLETED`, executa `MarkInventoryReserved()` e persiste a alteração.

Se já estiver em `INVENTORY_RESERVED`, continua o processamento sem repetir a transição.

Se estiver em `COMPLETED`, retorna sem executar novas operações.

### 7.3. Recuperação após `Order PAID`

O `PayOrderCommandHandler` verifica o estado do pedido antes de executar `order.Pay()`.

Quando o pedido já está `PAID`, retorna sem realizar outra transição e sem gerar outro `OrderPaidDomainEvent`.

Isso permite concluir a Saga mesmo quando o pagamento do pedido foi persistido antes de uma interrupção.

## 8. Integração com Inbox, Retry e DLQ

O Inbox registra o processamento de mensagens por `EventId`.

Quando ocorre uma exceção, a mensagem pode ser marcada como `FAILED`, enquanto o consumidor RabbitMQ aplica a política de Retry.

Após esgotar as tentativas, a mensagem pode ser encaminhada à Dead Letter Queue (DLQ).

O processamento de uma mensagem anteriormente marcada como `FAILED` pode ser retomado utilizando o mesmo `EventId`.

**Uma mensagem na DLQ não significa necessariamente que a Saga deve ser compensada.**

Ela pode representar uma operação parcialmente persistida e recuperável.

A decisão de compensar deve considerar o estado da Saga, os efeitos já persistidos e a natureza da falha.

## 9. Testes de recuperação realizados

Foram executados três cenários de falha simulada, utilizando exceções temporárias nos handlers e republicação controlada de mensagens pelo RabbitMQ Management.

| Cenário | Estado persistido antes da recuperação | Resultado |
|---|---|---|
| Falha após `PAYMENT_COMPLETED` | Saga em `PAYMENT_COMPLETED` | Saga retomada e concluída |
| Falha após `INVENTORY_RESERVED` | Saga em `INVENTORY_RESERVED`, pedido pendente | Pedido pago e Saga concluída |
| Falha após `Order PAID` | Pedido `PAID`, Saga em `INVENTORY_RESERVED` | Saga concluída sem repetir o pagamento |

Nos testes, foi confirmado que:

- As Sagas alcançaram `COMPLETED`.
- Os pedidos alcançaram `PAID`.
- As reservas permaneceram em `RESERVED`.
- Os eventos reprocessados passaram de `FAILED` para `PROCESSED` no Inbox.
- O campo `Error` do Inbox foi limpo após o processamento bem-sucedido.
- No terceiro cenário, foi encontrado exatamente um `OrderPaidDomainEvent` no Outbox para o pedido testado.

Esses resultados demonstram a recuperação sequencial nas três janelas de falha avaliadas.

## 10. Inicialização da topologia RabbitMQ

O `OrderFlow.Worker.Payments` registra o `RabbitMqTopologyHostedService`.

Esse serviço executa `RabbitMqTopologyInitializer.InitializeAsync()` durante a inicialização do Worker, antes do início dos consumidores registrados posteriormente.

A inicialização prepara a topologia necessária para a comunicação entre os participantes.

## 11. Limitações conhecidas

A implementação atual ainda possui pontos de evolução:

1. **Concorrência:** a idempotência foi validada em reprocessamentos sequenciais, não em execuções simultâneas.
2. **Compensações:** os comandos de estorno de pagamento e liberação de inventário ainda precisam ser integrados à orquestração.
3. **Falhas definitivas:** ainda não existe um fluxo completo para decidir e executar compensações a partir de falhas de negócio.
4. **Recuperação inicial:** o início da Saga e outras janelas de interrupção ainda exigem testes específicos.
5. **Consistência de mensagens:** permanecem necessários testes adicionais envolvendo redelivery, falhas de infraestrutura e publicação de eventos.

## 12. Próximas evoluções

A próxima etapa do OrderFlow será implementar as compensações da Saga.

O objetivo será tratar situações em que o processamento não pode continuar e seja necessário executar operações compensatórias, como:

```text
Payment COMPLETED
        |
        v
Inventory Reservation FAILED
        |
        v
Saga COMPENSATING
        |
        v
RefundPaymentCommand
        |
        v
Payment REFUNDED
        |
        v
Order CANCELLED
        |
        v
Saga COMPENSATED
```

Esse fluxo representa a evolução planejada, não uma funcionalidade já concluída.

## 13. Referências internas

- `docs/adr/ADR-009-Outbox-Pattern.md`
- `docs/adr/ADR-010-Inbox-Pattern.md`
- `docs/adr/ADR-011-Idempotencia.md`
- `docs/adr/ADR-012-Retry.md`
- `docs/adr/ADR-013-Dead-Letter-Queue.md`
- `docs/concepts/08-Outbox-Pattern.md`
- `docs/concepts/09-Inbox-Pattern.md`
- `docs/concepts/10-Idempotencia.md`
- `docs/concepts/11-Retry.md`
- `docs/concepts/12-Dead-Letter-Queue.md`
