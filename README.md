# 🚀 OrderFlow — Reliable Messaging Lab

> Um laboratório de arquitetura de software para construção de aplicações distribuídas utilizando **Clean Architecture**, **Domain-Driven Design (DDD)**, **CQRS** e **Event-Driven Architecture**.

---

# 📌 Sobre o Projeto

O **OrderFlow** é um projeto desenvolvido com o objetivo de estudar, praticar e demonstrar padrões arquiteturais utilizados em sistemas corporativos modernos.

Ao longo da evolução do projeto serão implementadas soluções utilizadas em aplicações distribuídas de alta disponibilidade, incluindo mensageria, consistência eventual, observabilidade e integração entre serviços.

Mais do que um CRUD, o OrderFlow busca demonstrar **como construir software escalável, desacoplado e preparado para ambientes distribuídos**.

O projeto evolui de forma incremental. Cada capítulo introduz novos conceitos arquiteturais, mantendo o código, a documentação e o histórico de commits sincronizados durante toda a evolução da solução.

Atualmente o projeto já possui as camadas **Domain**, **Application** e **Infrastructure** implementadas, incluindo persistência com Entity Framework Core, integração com SQL Server e versionamento do banco através de Migrations. As próximas etapas concentrarão esforços na exposição da aplicação via WebApi e, posteriormente, na implementação da arquitetura orientada a eventos utilizando RabbitMQ.

---

# 🎯 Objetivos

- Aplicar os princípios da Clean Architecture.
- Modelar um domínio rico utilizando Domain-Driven Design (DDD).
- Implementar CQRS utilizando MediatR.
- Trabalhar com Domain Events.
- Implementar persistência utilizando Entity Framework Core.
- Aplicar Repository e Unit of Work.
- Integrar serviços utilizando RabbitMQ.
- Implementar Outbox Pattern.
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
Client
   │
   ▼
WebApi
   │
   ▼
Application
   │
   ▼
Domain

Application
   │
   ▼
Infrastructure
   │
   ▼
SQL Server
```

Cada camada possui responsabilidades bem definidas.

| Camada | Responsabilidade |
|---------|------------------|
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
│   ├── OrderFlow.Domain
│   ├── OrderFlow.Application
│   ├── OrderFlow.Infrastructure
│   └── OrderFlow.WebApi
│
├── tests
│   ├── OrderFlow.Domain.Tests
│   └── OrderFlow.Application.Tests
│
└── docs
    ├── adr
    ├── concepts
    ├── decisions
    ├── diagrams
    └── Glossario.md
```

---

# 📚 Documentação

Toda a documentação do projeto está organizada na pasta **docs**.

| Pasta | Descrição |
|--------|-----------|
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
| Capítulo 6 — WebApi | ⏳ |
| Capítulo 7 — RabbitMQ | ⏳ |
| Capítulo 8 — Outbox Pattern | ⏳ |
| Capítulo 9 — Inbox Pattern | ⏳ |
| Capítulo 10 — Idempotência | ⏳ |
| Capítulo 11 — Arquitetura Distribuída | ⏳ |
| Capítulo 12 — Observabilidade | ⏳ |

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
| RabbitMQ | ⏳ |
| Outbox Pattern | ⏳ |
| Inbox Pattern | ⏳ |
| Idempotência | ⏳ |

---

## Arquitetura Distribuída

| Item | Status |
|------|:------:|
| Retry | ⏳ |
| Dead Letter Queue | ⏳ |
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

## Capítulo 5 — Infrastructure

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

## Capítulo 6 — WebApi

- Controllers
- Middlewares
- Swagger
- Tratamento global de exceções

## Capítulo 7 — RabbitMQ

- Event Publisher
- Event Consumer
- Integração assíncrona

## Capítulo 8

- Outbox Pattern

## Capítulo 9

- Inbox Pattern

## Capítulo 10

- Idempotência

## Capítulo 11

- Retry
- Dead Letter Queue
- Saga
- Consistência Eventual

## Capítulo 12

- Observabilidade
- OpenTelemetry
- Logs estruturados
- Métricas
- Distributed Tracing

---

# ▶️ Como Executar

> Em evolução.

A camada **Infrastructure** já se encontra implementada, incluindo Entity Framework Core, SQL Server, Migrations e User Secrets.

A documentação completa de execução será disponibilizada após a implementação da **WebApi**, permitindo a utilização dos endpoints da aplicação.

Todas as demais seções permanecem inalteradas.

---

# 📖 Licença

Projeto desenvolvido exclusivamente para fins de estudo, demonstração de arquitetura de software e evolução profissional.