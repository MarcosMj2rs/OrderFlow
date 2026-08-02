# Evolução da Arquitetura

Este documento apresenta a evolução arquitetural do **OrderFlow**,
demonstrando como a solução é construída de forma incremental ao longo
dos capítulos.

Cada etapa consolida uma parte da arquitetura antes da introdução de
novos conceitos, mantendo a aplicação aderente aos princípios da **Clean
Architecture**, **Domain-Driven Design (DDD)** e **CQRS**.

------------------------------------------------------------------------

# Capítulo 1 --- Estrutura da Solution

Foi criada a estrutura inicial da solução.

``` text
src
tests
docs
```

**Objetivos**

-   organização da solução;
-   separação entre camadas;
-   preparação para evolução incremental.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 2 --- Domain Model

Foi construída toda a camada **Domain**.

**Principais entregas**

-   Aggregate Root (`Order`);
-   Entity (`OrderItem`);
-   Domain Exceptions;
-   Repositórios do domínio;
-   Invariantes;
-   Testes unitários.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 3 --- Domain Events

Introdução dos eventos de domínio.

**Eventos implementados**

-   OrderCreatedDomainEvent;
-   OrderCancelledDomainEvent;
-   OrderPaidDomainEvent.

Também foi criada a infraestrutura para gerenciamento de Domain Events
através da classe base `Entity`.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 4 --- Application (CQRS)

Foi construída toda a camada **Application** utilizando CQRS.

**Commands** - CreateOrder; - CancelOrder; - PayOrder.

**Queries** - GetOrderById; - GetOrders.

**Behaviors** - ValidationBehavior.

**Abstrações** - IUnitOfWork; - IOrderRepository; -
IOrderReadRepository.

**Tecnologias** - MediatR; - FluentValidation; - Vertical Slice
Architecture.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 5 --- Infrastructure

Foi implementada toda a infraestrutura de persistência.

**Principais componentes**

### Entity Framework Core

-   OrderFlowDbContext;
-   Fluent API;
-   Entity Configurations;
-   Change Tracking;
-   Backing Fields;
-   Shadow Properties.

### Persistência

-   OrderRepository;
-   OrderReadRepository;
-   UnitOfWork.

### SQL Server

-   Integração com SQL Server;
-   Dependency Injection;
-   User Secrets.

### Banco de Dados

-   InitialCreate;
-   Migrations;
-   Índices;
-   Foreign Keys;
-   Model Snapshot.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 6 --- RabbitMQ (Publisher)

Foi implementada a primeira etapa da infraestrutura de mensageria.

-   RabbitMQ Configuration;
-   RabbitMqChannelFactory;
-   RabbitMqTopologyHostedService;
-   RabbitMqEventPublisher;
-   RabbitMqRoutingKeyResolver;
-   Exchanges;
-   Queues;
-   Routing Keys;
-   Publisher Confirms.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Capítulo 7 --- WebApi

Foi implementada a primeira versão da API REST.

-   OrdersController;
-   POST /orders;
-   GET /orders;
-   GET /orders/{id};
- 	PATCH /orders/{id}/pay
- 	PATCH /orders/{id}/cancel
-   API Versioning;
-   Swagger/OpenAPI;
-   AutoMapper;
-   Contracts;
-   Integração com MediatR.

**Status:** ✅ Concluído

------------------------------------------------------------------------

# Próxima etapa --- Processamento Assíncrono

-   Capítulo 8 — Consumers e Background Workers

**Status:** 🚧 Em desenvolvimento

------------------------------------------------------------------------

# Etapas Futuras

-   Outbox Pattern;
-   Inbox Pattern;
-   Idempotência;
-   Saga;
-   OpenTelemetry;
-   Observabilidade;
-   Testcontainers.

------------------------------------------------------------------------

# Evolução da Arquitetura

``` mermaid
flowchart TD
    A[Estrutura da Solution]
    --> B[Domain Model]
    --> C[Domain Events]
    --> D[Application CQRS]
    --> E[Infrastructure]
    --> F[RabbitMQ Publisher]
    --> G[WebApi]
    --> H[Consumers]
    --> I[Background Workers]
    --> J[Outbox]
    --> K[Inbox]
    --> L[Arquitetura Distribuída]
    --> M[Observabilidade]
```

------------------------------------------------------------------------

# Situação Atual

``` mermaid
flowchart LR
    subgraph Concluído
        A[Solution]
        B[Domain]
        C[Domain Events]
        D[Application]
        E[Infrastructure]
        F[RabbitMQ Publisher]
        G[WebApi]
    end

    subgraph Próximas Etapas
        H[Consumers]
        I[Workers]
        J[Outbox]
        K[Inbox]
        L[Observabilidade]
    end

    G --> H
```

------------------------------------------------------------------------

# Observações

O OrderFlow é desenvolvido de forma incremental. Cada capítulo consolida
completamente uma etapa da arquitetura antes da introdução de novos
componentes, garantindo a evolução consistente do código, da
documentação e das decisões arquiteturais.
