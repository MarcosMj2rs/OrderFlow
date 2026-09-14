# 🚀 OrderFlow — Reliable Messaging Lab

> Um laboratório de arquitetura de software para construção de aplicações distribuídas utilizando **Clean Architecture**, **Domain-Driven Design (DDD)**, **CQRS** e **Event-Driven Architecture**.

---

# 📌 Sobre o Projeto

O **OrderFlow** é um projeto desenvolvido com o objetivo de estudar, praticar e demonstrar padrões arquiteturais utilizados em sistemas corporativos modernos.

Ao longo da evolução do projeto serão implementadas soluções utilizadas em aplicações distribuídas de alta disponibilidade, incluindo mensageria, consistência eventual, observabilidade e integração entre serviços.

Mais do que um CRUD, o OrderFlow busca demonstrar **como construir software escalável, desacoplado e preparado para ambientes distribuídos**.

O projeto evolui de forma incremental. Cada capítulo introduz novos conceitos arquiteturais, mantendo o código, a documentação e o histórico de commits sincronizados durante toda a evolução da solução.

Atualmente o projeto já possui uma arquitetura distribuída funcional baseada em **RabbitMQ**, incluindo **Transactional Outbox**, **Inbox Pattern**, **Idempotência**, **Retry**, **Dead Letter Queue (DLQ)** e **Background Workers**.

Os próximos capítulos concentrarão esforços na implementação de **Saga**, **Consistência Eventual**, **Versionamento de Eventos**, **Observabilidade** e demais padrões utilizados em sistemas distribuídos corporativos.

---

# 🎯 Objetivos

- Aplicar os princípios da Clean Architecture.
- Modelar um domínio rico utilizando Domain-Driven Design (DDD).
- Implementar CQRS utilizando MediatR.
- Trabalhar com Domain Events.
- Implementar persistência utilizando Entity Framework Core.
- Aplicar Repository e Unit of Work.
- Integrar serviços utilizando RabbitMQ.
- Implementar Transactional Outbox Pattern.
- Garantir At Least Once Delivery.
- Implementar Inbox Pattern.
- Garantir Idempotência.
- Demonstrar estratégias de Retry.
- Trabalhar com Dead Letter Queue (DLQ).
- Implementar Sagas.
- Demonstrar Consistência Eventual.
- Implementar Observabilidade utilizando OpenTelemetry.

---

# ⚙️ Stack Tecnológica

| Categoria | Tecnologia |
|-----------|------------|
| Linguagem | C# |
| Plataforma | .NET 10 |
| Arquitetura | Clean Architecture |
| Modelagem | Domain-Driven Design |
| Application | MediatR + FluentValidation |
| ORM | Entity Framework Core |
| Banco de Dados | SQL Server |
| Mensageria | RabbitMQ |
| Testes | xUnit + FluentAssertions + Moq |
| Containers | Docker + Testcontainers |
| Observabilidade | OpenTelemetry |

---

# 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, mantendo as dependências sempre apontando para o centro da aplicação.

```text
Client (Swagger)
        │
        ▼
OrderFlow.Api
    │
    ├── Controllers
    ├── Contracts
    ├── AutoMapper
    └── API Versioning
        │
        ▼
Application
    │
    ├── Commands
    ├── Queries
    ├── Handlers
    ├── Validators
    └── Behaviors
        │
        ▼
Domain
    │
    ├── Aggregate Root
    ├── Entities
    ├── Domain Events
    └── Business Rules
        │
        ▼
Infrastructure
    │
    ├── EF Core
    ├── SQL Server
    ├── UnitOfWork
    ├── Transactional Outbox
    ├── Inbox
    ├── RabbitMQ
    └── Persistence
        │
        ▼
SQL Server
    │
    ├── Orders
    ├── OrderItems
    ├── Payments
    ├── InboxMessages
    └── OutboxMessages
            │
            ▼
OrderFlow.Worker.Outbox
            │
            ▼
RabbitMQ
            │
            ▼
OrderFlow.Worker.Payments
```

Cada camada possui responsabilidades bem definidas.

| Camada | Responsabilidade |
|--------|------------------|
| Domain | Regras de negócio |
| Application | Casos de uso |
| Infrastructure | Persistência e integrações |
| WebApi | Exposição da aplicação via HTTP |

---

# 📁 Estrutura da Solution

```text
OrderFlow
│
├── src
│   ├── OrderFlow.Api
│   ├── OrderFlow.Domain
│   ├── OrderFlow.Application
│   ├── OrderFlow.Infrastructure
│   ├── OrderFlow.Worker.Outbox
│   ├── OrderFlow.Worker.Payments
│   └── OrderFlow.Worker.Inbox
│
├── tests
│   ├── OrderFlow.Domain.Tests
│   ├── OrderFlow.Application.Tests
│   └── OrderFlow.Integration.Tests
│
└── docs
    ├── adr
    ├── concepts
    ├── decisions
    ├── diagrams
    ├── images
    └── Glossario.md
```

---

# 📚 Documentação

Toda a documentação do projeto está organizada na pasta **docs**.

| Pasta | Descrição |
|-------|-----------|
| concepts | Conceitos utilizados durante o desenvolvimento |
| adr | Architecture Decision Records (ADRs) |
| decisions | Comparativos e justificativas técnicas |
| diagrams | Diagramas em Mermaid |
| Glossario | Definições dos principais termos do projeto |

---

# 📖 Evolução do Projeto

O desenvolvimento do OrderFlow foi dividido em capítulos, permitindo acompanhar a evolução da arquitetura de forma incremental.

| Capítulo | Status |
|----------|:------:|
| Capítulo 1 — Estrutura da Solution | ✅ |
| Capítulo 2 — Domain Model | ✅ |
| Capítulo 3 — Domain Events | ✅ |
| Capítulo 4 — Application (CQRS) | ✅ |
| Capítulo 5 — Infrastructure | ✅ |
| Capítulo 6 — RabbitMQ | ✅ |
| Capítulo 7 — Background Workers | ✅ |
| Capítulo 8 — WebApi | ✅ |
| Capítulo 9 — Transactional Outbox | ✅ |
| Capítulo 10 — Inbox Pattern | ✅ |
| Capítulo 11 — Idempotência | ✅ |
| Capítulo 12 — Retry | ✅ |
| Capítulo 13 — Dead Letter Queue (DLQ) | ✅ |
| Capítulo 14 — Saga | ⏳ |
| Capítulo 15 — Observabilidade | ⏳ |

---

# 📊 Estado Atual do Projeto

## Domain

**Status da camada:** ✅ Concluída

| Item | Status |
|------|:------:|
| Domain Model | ✅ |
| Aggregate Root | ✅ |
| Domain Events | ✅ |
| Repositórios | ✅ |
| Testes Unitários | ✅ |

---

## Application

**Status da camada:** ✅ Concluída

### Commands

| Caso de Uso | Status |
|-------------|:------:|
| CreateOrder | ✅ |
| CancelOrder | ✅ |
| PayOrder | ✅ |

### Queries

| Caso de Uso | Status |
|-------------|:------:|
| GetOrderById | ✅ |
| GetOrders | ✅ |

### Behaviors

| Item | Status |
|------|:------:|
| ValidationBehavior | ✅ |
| LoggingBehavior | ⏳ |
| PerformanceBehavior | ⏳ |
| ExceptionHandlingBehavior | ⏳ |

### Abstrações

| Item | Status |
|------|:------:|
| IUnitOfWork | ✅ |
| IOrderRepository | ✅ |
| IOrderReadRepository | ✅ |

### Componentes

| Item | Status |
|------|:------:|
| MediatR | ✅ |
| FluentValidation | ✅ |
| Vertical Slice Architecture | ✅ |

---

## Infrastructure

**Status da camada:** ✅ Concluída

| Item | Status |
|------|:------:|
| Entity Framework Core | ✅ |
| SQL Server | ✅ |
| DbContext | ✅ |
| Entity Configurations | ✅ |
| Repository | ✅ |
| Read Repository | ✅ |
| Unit of Work | ✅ |
| Dependency Injection | ✅ |
| User Secrets | ✅ |
| Migrations | ✅ |
| RabbitMQ | ✅ |
| RabbitMQ Topology | ✅ |
| Publisher | ✅ |
| Consumer | ✅ |
| Background Workers | ✅ |
| Retry | ✅ |
| Dead Letter Queue (DLQ) | ✅ |
| Transactional Outbox | ✅ |
| Inbox Pattern | ✅ |
| Idempotência | ✅ |

---

## Arquitetura Distribuída

| Item | Status |
|------|:------:|
| Event-Driven Architecture | ✅ |
| Transactional Outbox | ✅ |
| At Least Once Delivery | ✅ |
| Retry | ✅ |
| Dead Letter Queue (DLQ) | ✅ |
| Inbox Pattern | ✅ |
| Idempotência | ✅ |
| Saga | ⏳ |
| Consistência Eventual | ⏳ |
| Versionamento de Eventos | ⏳ |

---

## Observabilidade

| Item | Status |
|------|:------:|
| Logs Estruturados | ⏳ |
| Métricas | ⏳ |
| Tracing Distribuído | ⏳ |
| OpenTelemetry | ⏳ |

---

## Testes

| Tipo | Status |
|------|:------:|
| Testes Unitários do Domain | ✅ |
| Testes Unitários da Application | ✅ |
| Testes de Integração | ⏳ |
| Testcontainers | ⏳ |

---

# 🛣️ Roadmap

## ✅ Capítulo 5 — Infrastructure

- Entity Framework Core
- DbContext
- Fluent API
- Entity Configurations
- Repositories
- Read Repository
- Unit of Work
- SQL Server
- User Secrets
- Migrations

## ✅ Capítulo 6 — RabbitMQ

- Fundamentos de mensageria
- Connection
- Channel
- Exchange
- Queue
- Binding
- Routing Key
- Publisher
- Publisher Confirm

## ✅ Capítulo 7 — Background Workers

- Consumer
- Ack
- Nack
- Reject
- Prefetch

## ✅ Capítulo 8 — WebApi

- Controllers
- Middlewares
- Swagger
- Tratamento global de exceções

## ✅ Capítulo 9 — Transactional Outbox

- OutboxMessage
- OutboxMessageFactory
- OutboxRepository
- OutboxEventTypeRegistry
- OutboxPublisherService
- OrderFlow.Worker.Outbox
- Persistência transacional
- Publicação assíncrona
- Recuperação automática
- Validação dos cenários de resiliência

## ✅ Capítulo 10 — Inbox Pattern

- InboxMessage
- InboxRepository
- InboxProcessor
- Controle de mensagens processadas
- Idempotência no consumo

## ✅ Capítulo 11 — Idempotência

- Idempotência dos Consumers
- Chaves de idempotência
- Estratégias de deduplicação
- Idempotência de negócio
- Proteção contra concorrência

## ✅ Capítulo 12 — Retry

- Classificação de falhas transitórias
- Retry Count
- Exponential Backoff
- Retry Queue
- Expiration por mensagem
- Limite máximo de tentativas
- Publisher Confirm antes do ACK
- Validação dos cenários de Retry

## ✅ Capítulo 13 — Dead Letter Queue (DLQ)

- Classificação de falhas permanentes
- Encaminhamento direto para DLQ
- Encaminhamento após esgotamento do Retry
- Preservação do payload e metadados
- Retry Count
- Isolamento das mensagens
- Publisher Confirm antes do ACK
- Validação dos cenários de DLQ

## ⏳ Capítulo 14 — Saga

- Saga Orquestrada
- Saga Coreografada
- Compensações
- Consistência Eventual
- Versionamento de Eventos

## ⏳ Capítulo 15 — Observabilidade

- Logs estruturados
- Métricas
- OpenTelemetry
- Distributed Tracing

---

# ▶️ Como Executar

> Em evolução.

A solução já possui uma arquitetura distribuída funcional utilizando SQL Server, RabbitMQ, Transactional Outbox, Inbox Pattern, Idempotência, Retry e Dead Letter Queue.

A documentação de execução continuará sendo atualizada conforme a evolução dos próximos capítulos.

---

# 📖 Licença

Projeto desenvolvido exclusivamente para fins de estudo, demonstração de arquitetura de software e evolução profissional.