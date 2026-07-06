# 🚀 OrderFlow

> Um laboratório de arquitetura de software para construção de aplicações distribuídas utilizando **Clean Architecture**, **DDD**, **CQRS** e **Event-Driven Architecture**.

---

# 📌 Sobre o Projeto

O **OrderFlow** é um projeto desenvolvido com o objetivo de estudar e aplicar padrões arquiteturais utilizados em sistemas corporativos modernos.

Ao longo da evolução do projeto serão implementadas soluções utilizadas em aplicações distribuídas de alta disponibilidade, incluindo mensageria, consistência eventual, observabilidade e integração entre serviços.

Mais do que um CRUD, o OrderFlow busca demonstrar **como construir software escalável, desacoplado e preparado para ambientes distribuídos**.

---

# 🎯 Objetivos

- Aplicar os princípios da Clean Architecture.
- Modelar um domínio rico utilizando Domain-Driven Design (DDD).
- Implementar CQRS utilizando MediatR.
- Trabalhar com Domain Events.
- Implementar persistência com Entity Framework Core.
- Aplicar Repository e Unit of Work.
- Integrar serviços utilizando RabbitMQ.
- Implementar Outbox Pattern.
- Implementar Inbox Pattern.
- Garantir Idempotência.
- Demonstrar estratégias de Retry.
- Trabalhar com Dead Letter Queue (DLQ).
- Demonstrar Sagas e Consistência Eventual.
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
| Persistência | Entity Framework Core |
| Banco de Dados | SQL Server |
| Mensageria | RabbitMQ |
| Testes | xUnit + FluentAssertions + Moq |
| Containers | Docker + Testcontainers |
| Observabilidade | OpenTelemetry |

---

# 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, mantendo dependências sempre apontando para o centro da aplicação.

```text
Client

↓

WebApi

↓

Application

↓

Domain

↓

Infrastructure

↓

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

# 📁 Estrutura da Solução

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
| concepts | Conceitos utilizados no projeto |
| adr | Architecture Decision Records |
| decisions | Comparativos e justificativas técnicas |
| diagrams | Diagramas em Mermaid |
| Glossario | Termos utilizados durante o projeto |

---

# 📊 Estado Atual do Projeto

## Foundation

| Item | Status |
|------|:------:|
| Estrutura da Solution | ✅ |
| Documentação Inicial | ✅ |
| Domain Model | ✅ |
| Domain Events | ✅ |
| Testes Unitários do Domínio | ✅ |

---

## Application

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

### Infraestrutura da Application

| Item | Status |
|------|:------:|
| MediatR | ✅ |
| FluentValidation | ✅ |
| IUnitOfWork | ✅ |
| IOrderRepository | ✅ |

---

## Infrastructure

| Item | Status |
|------|:------:|
| Entity Framework Core | ⏳ |
| SQL Server | ⏳ |
| Repository | ⏳ |
| Unit of Work | ⏳ |
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
| Testes Unitários do Domínio | ✅ |
| Testes Unitários da Application | ✅ |
| Testes de Integração | ⏳ |
| Testcontainers | ⏳ |

---

# 🛣️ Roadmap

As próximas etapas de evolução do projeto serão:

1. Infrastructure (Entity Framework Core)
2. SQL Server
3. Repository e Unit of Work
4. WebApi
5. RabbitMQ
6. Outbox Pattern
7. Inbox Pattern
8. Idempotência
9. Retry
10. Dead Letter Queue
11. Saga
12. Observabilidade
13. OpenTelemetry
14. Testcontainers

---

# ▶️ Como Executar

> Em construção.

Esta seção será concluída após a implementação da camada Infrastructure e da WebApi.

---

# 📖 Licença

Projeto desenvolvido exclusivamente para fins de estudo, experimentação de arquitetura de software e evolução profissional.