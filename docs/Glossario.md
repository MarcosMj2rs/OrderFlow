# Glossário — OrderFlow

Este documento reúne os principais termos utilizados durante o desenvolvimento do **OrderFlow**, um laboratório de arquitetura distribuída em **.NET 10**.

As definições são resumidas. Para aprofundamento, consulte `docs/concepts`, os registros em `docs/adr` e os diagramas em `docs/diagrams`.

> **Situação do Capítulo 14:** o fluxo de sucesso da Saga orquestrada, a persistência de estado, a idempotência e cenários de recuperação foram implementados e validados. As compensações de pagamento e estoque permanecem pendentes.

---

## A

### ACK (Acknowledgement)

Confirmação enviada pelo consumidor ao RabbitMQ indicando que uma entrega foi processada com sucesso. Um ACK não equivale, por si só, à confirmação de toda a transação de negócio.

### Aggregate

Conjunto de entidades e objetos de valor que devem permanecer consistentes como uma unidade de negócio. No OrderFlow, o Aggregate de pedido é formado por `Order` e seus `OrderItems`.

### Aggregate Root

Entidade que controla o acesso ao Aggregate e protege suas invariantes. No Aggregate de pedido do OrderFlow, a raiz é `Order`.

### AsNoTracking

Configuração do Entity Framework Core que desabilita o rastreamento das entidades retornadas por uma consulta. É utilizada pelo `OrderReadRepository` em operações de leitura.

### At Least Once Delivery

Modelo de entrega em que uma mensagem pode chegar uma ou mais vezes. Exige tratamento de duplicidades no consumidor, como Inbox e idempotência; não garante processamento exatamente uma vez.

---

## B

### Backing Field

Campo privado utilizado pelo Entity Framework Core para persistir o estado sem expor coleções mutáveis. No Aggregate `Order`, `_items` é o backing field da propriedade `Items`.

### Background Worker

Processo que executa tarefas em segundo plano. O OrderFlow utiliza Workers para publicação de mensagens da Outbox e processamento assíncrono.

### Binding

Associação entre uma Exchange e uma Queue no RabbitMQ, normalmente utilizando uma Routing Key.

---

## C

### Cascade Delete

Comportamento pelo qual registros dependentes são excluídos quando o registro principal é excluído fisicamente. No OrderFlow, uma exclusão física de `Order` pode remover seus `OrderItems`; cancelar um pedido não é uma exclusão física.

### Change Tracking

Mecanismo do Entity Framework Core que acompanha alterações em entidades carregadas pelo `DbContext`, permitindo persistir mudanças com `SaveChangesAsync()`.

### Clean Architecture

Organização arquitetural que separa responsabilidades e mantém regras de negócio independentes de tecnologias externas. No OrderFlow, as camadas centrais não dependem diretamente de SQL Server, RabbitMQ ou HTTP.

### Command

Objeto que representa uma intenção de alterar o estado do sistema. Exemplos do OrderFlow: `CreateOrder`, `CancelOrder` e `PayOrder`. Os respectivos Handlers coordenam casos de uso e podem provocar Domain Events.

### Compensação (Saga)

Operação de negócio que procura desfazer ou neutralizar os efeitos de uma etapa previamente concluída de uma transação distribuída. Exemplos planejados para o OrderFlow são **estorno de pagamento** e **liberação de reserva de estoque**. **Essas compensações ainda não foram implementadas.**

Uma compensação não é o mesmo que um rollback transacional de banco de dados.

### Consistência Eventual

Modelo no qual componentes distribuídos podem observar estados temporariamente diferentes, convergindo após o processamento das mensagens e das etapas necessárias. A Saga coordena transações locais sem exigir uma transação distribuída única.

### Consumer

Componente que recebe mensagens de uma fila e executa seu processamento. No OrderFlow, consumidores concretos utilizam a infraestrutura compartilhada de mensageria.

### CQRS (Command Query Responsibility Segregation)

Padrão que separa operações de escrita (**Commands**) das operações de leitura (**Queries**), permitindo modelos e responsabilidades distintos.

---

## D

### DbContext

Classe central do Entity Framework Core que representa uma sessão de persistência. O `OrderFlowDbContext` gerencia mapeamentos, Change Tracking e gravação de dados.

### Dead Letter Queue (DLQ)

Fila destinada a isolar mensagens que não puderam ser processadas normalmente, por exemplo, após falhas permanentes ou esgotamento das tentativas de Retry. Encaminhar uma mensagem à DLQ **não executa automaticamente uma compensação de negócio**.

### Domain Event

Evento que representa um fato relevante ocorrido no domínio. Exemplos: `OrderCreatedDomainEvent`, `OrderCancelledDomainEvent` e `OrderPaidDomainEvent`.

### Domain Exception

Exceção que representa a violação de uma regra de negócio do domínio.

### Domain-Driven Design (DDD)

Abordagem de modelagem que coloca as regras e a linguagem do negócio no centro do desenvolvimento. No OrderFlow, inclui Aggregates, entidades, invariantes e Domain Events.

---

## E

### Eager Loading

Estratégia de carregamento que recupera entidades relacionadas junto da entidade principal. No OrderFlow, o `OrderRepository` carrega o pedido com seus itens para operar sobre o Aggregate completo.

### Entity

Objeto de domínio com identidade própria ao longo de seu ciclo de vida. Exemplos: `Order` e `OrderItem`.

### Entity Configuration

Classe que concentra mapeamentos do Entity Framework Core por meio da Fluent API. Exemplos: `OrderConfiguration` e `OrderItemConfiguration`.

### Event-Driven Architecture

Estilo arquitetural em que componentes se comunicam por eventos. No OrderFlow, a mensageria assíncrona utiliza RabbitMQ e padrões de confiabilidade como Outbox e Inbox.

### Exchange

Componente do RabbitMQ que recebe mensagens publicadas e as encaminha às filas conforme sua configuração e as Routing Keys.

---

## F

### Fluent API

API do Entity Framework Core para configurar entidades, propriedades, índices e relacionamentos sem introduzir atributos de persistência no domínio.

### Foreign Key

Restrição de banco de dados que relaciona registros e preserva a integridade referencial. No OrderFlow, `OrderItems.OrderId` referencia `Orders.Id`.

---

## H

### Handler

Componente responsável por coordenar a execução de um Command ou Query, acessando abstrações e solicitando persistência quando necessário. As regras e invariantes do negócio permanecem no domínio.

---

## I

### Idempotência

Propriedade pela qual repetir uma operação com a mesma identidade não produz efeitos de negócio adicionais indevidos. No OrderFlow, é importante tanto para o consumo de mensagens quanto para as etapas de pagamento e reserva de estoque da Saga.

### Inbox Pattern

Padrão que registra e controla o processamento de mensagens recebidas para evitar efeitos duplicados e permitir tratamento de falhas. No OrderFlow, há cenário validado de reprocessamento de mensagem de `FAILED` para `PROCESSED`.

### Integration Event

Mensagem usada para comunicar acontecimentos entre componentes ou serviços. É conceitualmente distinta de um Domain Event, embora um evento de domínio possa originar uma mensagem de integração.

### Invariante

Regra que deve permanecer verdadeira ao longo do ciclo de vida de um Aggregate. Exemplos: um pedido deve possuir itens; um pedido cancelado não pode receber novos itens; o total deve corresponder à soma dos itens.

---

## M

### MediatR

Biblioteca utilizada para despachar Commands e Queries aos respectivos Handlers e executar Pipeline Behaviors na camada Application.

### Migration

Arquivo do Entity Framework Core que descreve mudanças no esquema do banco de dados. A migration inicial do OrderFlow é `InitialCreate`.

### Model Snapshot

Representação do modelo conhecida pelo Entity Framework Core, utilizada na comparação para gerar novas migrations. Não deve ser alterada manualmente.

---

## O

### Orquestrador de Saga

Componente que coordena explicitamente a sequência de etapas de uma Saga, acompanha seu estado e determina a próxima ação conforme os resultados. No OrderFlow, foi adotada a abordagem **orquestrada**, não a coreografada.

### OrderSaga

Registro persistido que representa o estado da execução da Saga de um pedido. Sua persistência permite identificar etapas já concluídas e apoiar a recuperação após interrupções.

### Outbox Pattern (Transactional Outbox)

Padrão que grava eventos destinados à publicação juntamente com as alterações de negócio em uma transação local de banco de dados. Um Worker publica posteriormente as mensagens no RabbitMQ. Reduz a janela de inconsistência entre gravação e publicação, mas não elimina a possibilidade de entregas duplicadas.

---

## P

### Pipeline Behavior

Componente do MediatR que executa lógica transversal ao redor de um Handler. No OrderFlow, `ValidationBehavior` valida as entradas antes da execução.

### Prefetch (QoS)

Configuração do RabbitMQ que limita a quantidade de mensagens entregues a um consumidor sem confirmação, auxiliando no controle de carga.

### Publisher

Componente responsável por publicar mensagens em uma Exchange do RabbitMQ.

### Publisher Confirm

Mecanismo de confirmação pelo broker de que recebeu uma publicação. É relevante para a confiabilidade do Outbox e da republicação em fluxos de Retry e DLQ.

---

## Q

### Query

Objeto que representa uma operação de leitura. No OrderFlow, as Queries retornam modelos específicos e não devem alterar o estado do domínio.

### Queue

Fila do RabbitMQ que armazena mensagens até sua entrega aos consumidores, segundo a configuração e as garantias do broker.

---

## R

### RabbitMQ

Broker de mensagens utilizado pelo OrderFlow para comunicação assíncrona, com Exchanges, Queues, Bindings e Routing Keys.

### Read Model

Modelo de dados destinado à leitura. No OrderFlow, Queries retornam DTOs, sem expor diretamente as entidades de domínio.

### Read Repository

Abstração especializada em consultas. No OrderFlow, é representada por `IOrderReadRepository`.

### Recuperação de Saga

Retomada ou reexecução controlada de uma Saga após interrupção técnica, considerando o estado persistido e a idempotência das etapas. No OrderFlow, foram validados cenários de interrupção após `PAYMENT_COMPLETED`, após `INVENTORY_RESERVED` e após o pedido atingir `PAID` antes de a Saga ser marcada como `COMPLETED`.

### Repository

Abstração de acesso aos Aggregates. O `IOrderRepository` é utilizado pelos casos de uso de escrita para carregar e persistir o Aggregate `Order`.

### Retry

Estratégia de repetir o processamento de uma operação ou mensagem após falhas recuperáveis, respeitando uma política de tentativas. Retry técnico não substitui compensação de negócio.

### Routing Key

Identificador usado pelo RabbitMQ para decidir, em conjunto com o tipo de Exchange e seus Bindings, para quais filas uma mensagem será encaminhada.

---

## S

### Saga Pattern

Padrão de coordenação de uma operação de negócio distribuída composta por **transações locais**. Cada etapa confirma suas próprias alterações, enquanto a Saga acompanha o progresso e pode, quando implementado, acionar compensações em caso de falha de negócio.

No OrderFlow, a Saga é **orquestrada** e coordena o fluxo de pagamento, reserva de estoque e conclusão do pedido. O fluxo de sucesso, a persistência e a recuperação técnica foram validados; **as compensações ainda estão pendentes**.

Consulte `docs/concepts/13-Saga.md` e `docs/adr/ADR-017-Saga-Pattern.md`.

### Saga Coreografada

Modelo de Saga em que os participantes reagem a eventos sem um orquestrador central responsável pela sequência completa. Foi considerada como alternativa arquitetural, mas não é a abordagem adotada no OrderFlow.

### Shadow Property

Propriedade existente apenas no modelo interno do Entity Framework Core, sem declaração na classe de domínio. No OrderFlow, `OrderId` é utilizada como Shadow Property para o relacionamento de `OrderItem`.

### SQL Server

Banco de dados relacional utilizado para persistir os dados do OrderFlow, incluindo pedidos, mensagens de confiabilidade e estado da Saga.

---

## T

### Transação Local

Conjunto de alterações confirmadas atomicamente dentro de um único recurso transacional, como o SQL Server. Uma Saga combina várias transações locais, sem pressupor uma transação distribuída única.

---

## U

### Unit of Work

Abstração responsável por confirmar as alterações de um caso de uso. No OrderFlow, `IUnitOfWork` desacopla a Application da implementação de persistência com Entity Framework Core.

### User Secrets

Recurso de desenvolvimento do .NET para armazenar configurações sensíveis fora do código-fonte. No OrderFlow, é utilizado para a Connection String local do SQL Server.

---

## V

### ValidationBehavior

Pipeline Behavior que executa Validators registrados antes de chamar o Handler. Havendo falha de validação, o Handler não é executado.

### Validator

Componente que valida dados de entrada de Commands e Queries. No OrderFlow, utiliza FluentValidation; regras de negócio continuam encapsuladas no domínio.

### Vertical Slice Architecture

Estratégia de organização que agrupa os componentes por caso de uso. Exemplo:

```text
CreateOrder
├── CreateOrderCommand.cs
├── CreateOrderCommandHandler.cs
├── CreateOrderCommandValidator.cs
└── CreateOrderResponse.cs
```

Facilita a evolução independente de funcionalidades e reforça a coesão de cada caso de uso.

---

## Referências internas

- `docs/concepts/08-Outbox-Pattern.md`
- `docs/concepts/09-Inbox-Pattern.md`
- `docs/concepts/10-Idempotencia.md`
- `docs/concepts/11-Retry.md`
- `docs/concepts/12-Dead-Letter-Queue.md`
- `docs/concepts/13-Saga.md`
- `docs/adr/ADR-017-Saga-Pattern.md`
- `docs/diagrams/Arquitetura-Geral.md`
- `docs/diagrams/Fluxo-Pagamento.md`
