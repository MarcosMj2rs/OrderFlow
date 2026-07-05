# 🚀 OrderFlow - Reliable Messaging Lab

## 📌 Sobre o Projeto

O **OrderFlow** é um projeto de estudos desenvolvido para demonstrar a construção de uma aplicação distribuída utilizando **Clean Architecture**, **Domain-Driven Design (DDD)** e **CQRS**, evoluindo gradualmente até uma arquitetura orientada a eventos com **RabbitMQ**, **Outbox Pattern**, **Inbox Pattern**, **Idempotência** e **Consistência Eventual**.

Mais do que um CRUD, este projeto busca representar práticas utilizadas em sistemas corporativos de alta disponibilidade e processamento assíncrono.

---

# 🎯 Objetivos

- Aplicar os princípios da Clean Architecture.
- Modelar um domínio rico utilizando DDD.
- Implementar CQRS com MediatR.
- Demonstrar Domain Events.
- Implementar persistência utilizando Entity Framework Core.
- Utilizar SQL Server como banco de dados.
- Implementar comunicação assíncrona com RabbitMQ.
- Garantir consistência utilizando Outbox Pattern.
- Garantir processamento único utilizando Inbox Pattern.
- Implementar Idempotência.
- Demonstrar estratégias de Retry e Dead Letter Queue.
- Implementar Observabilidade utilizando OpenTelemetry.

---

# ⚙️ Stack Tecnológica

- .NET 10
- C#
- Entity Framework Core
- SQL Server
- RabbitMQ
- MediatR
- FluentValidation
- xUnit
- FluentAssertions
- Moq
- Docker
- Testcontainers
- OpenTelemetry

---

# 🏗️ Arquitetura da Solução

O projeto segue os princípios da **Clean Architecture**.

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

A comunicação entre as camadas ocorre exclusivamente através de abstrações, mantendo o domínio completamente independente da infraestrutura.

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

Toda a documentação do projeto está organizada na pasta `docs`.

Ela está dividida em:

- **Concepts** — Conceitos técnicos utilizados no projeto.
- **ADR (Architecture Decision Records)** — Decisões arquiteturais tomadas durante o desenvolvimento.
- **Decisions** — Comparativos e justificativas das principais escolhas técnicas.
- **Diagrams** — Diagramas em Mermaid descrevendo os fluxos da aplicação.
- **Glossário** — Definições dos principais termos utilizados no projeto.

---

# 📊 Funcionalidades Implementadas

## Foundation

| Item | Status |
|------|:------:|
| Estrutura da Solution | ✅ |
| Documentação Inicial | ✅ |
| Modelagem do Domínio | ✅ |
| Testes Unitários do Domínio | ✅ |
| Domain Events | ✅ |

---

## Application

### Commands

| Caso de Uso | Status |
|-------------|:------:|
| CreateOrder | ✅ |
| CancelOrder | ✅ |
| PayOrder | ⏳ |

### Queries

| Consulta | Status |
|-----------|:------:|
| GetOrderById | ⏳ |
| GetOrders | ⏳ |

### Behaviors

| Item | Status |
|------|:------:|
| ValidationBehavior | ✅ |
| LoggingBehavior | ⏳ |
| PerformanceBehavior | ⏳ |
| ExceptionHandlingBehavior | ⏳ |

### Infraestrutura da Application

| Item | Status |
|------|:------:|
| MediatR | ✅ |
| FluentValidation | ✅ |
| UnitOfWork (Abstração) | ✅ |

---

## Infrastructure

| Item | Status |
|------|:------:|
| Entity Framework Core | ⏳ |
| SQL Server | ⏳ |
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
| OpenTelemetry | ⏳ |
| Logs Estruturados | ⏳ |
| Métricas | ⏳ |
| Tracing Distribuído | ⏳ |

---

## Testes

| Item | Status |
|------|:------:|
| Testes Unitários do Domínio | ✅ |
| Testes Unitários da Application | ✅ |
| Testes de Integração | ⏳ |
| Testcontainers | ⏳ |

---

# 🛣️ Roadmap

Os próximos capítulos do projeto serão desenvolvidos na seguinte ordem:

1. Finalização da camada Application
2. Entity Framework Core
3. SQL Server
4. RabbitMQ
5. Outbox Pattern
6. Inbox Pattern
7. Idempotência
8. Retry
9. Dead Letter Queue
10. Saga
11. Observabilidade
12. OpenTelemetry
13. Versionamento de Eventos
14. Testcontainers

---

# ▶️ Como Executar

> Em construção.

A documentação de execução será adicionada após a implementação da camada Infrastructure.

---

# 📖 Licença

Projeto desenvolvido exclusivamente para fins de estudo, demonstração de arquitetura de software e evolução profissional.