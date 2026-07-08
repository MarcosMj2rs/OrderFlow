# Evolução da Arquitetura

Este documento apresenta a evolução arquitetural do **OrderFlow**, mostrando como a solução cresce de forma incremental ao longo dos capítulos.

Cada etapa introduz novos conceitos, mantendo a arquitetura consistente e aderente aos princípios da **Clean Architecture**, **Domain-Driven Design (DDD)** e **CQRS**.

---

# Capítulo 1 — Estrutura da Solution

Foi criada a estrutura inicial da solução.

```text
src
tests
docs
```

Objetivos:

- organização da solução;
- separação entre camadas;
- preparação para evolução incremental.

**Status:** ✅ Concluído

---

# Capítulo 2 — Domain Model

Foi construída toda a camada **Domain**.

Principais entregas:

- Aggregate Root (`Order`);
- Entity (`OrderItem`);
- Domain Exceptions;
- Repositórios do domínio;
- Invariantes;
- Testes unitários.

**Status:** ✅ Concluído

---

# Capítulo 3 — Domain Events

Introdução dos eventos de domínio.

Eventos implementados:

- OrderCreatedDomainEvent
- OrderCancelledDomainEvent
- OrderPaidDomainEvent

Também foi criada a infraestrutura para gerenciamento de Domain Events através da classe base `Entity`.

**Status:** ✅ Concluído

---

# Capítulo 4 — Application (CQRS)

Foi construída toda a camada **Application** utilizando CQRS.

Principais componentes implementados:

## Commands

- CreateOrder
- CancelOrder
- PayOrder

## Queries

- GetOrderById
- GetOrders

## Behaviors

- ValidationBehavior

## Abstrações

- IUnitOfWork
- IOrderRepository
- IOrderReadRepository

## Tecnologias

- MediatR
- FluentValidation
- Vertical Slice Architecture

Ao término deste capítulo, a camada **Application** encontra-se completamente implementada e desacoplada da infraestrutura.

**Status:** ✅ Concluído

---

# Próxima etapa — Infrastructure

O próximo capítulo será responsável por implementar as abstrações definidas até aqui.

Serão desenvolvidos:

- Entity Framework Core;
- DbContext;
- Entity Configurations;
- Repositories;
- UnitOfWork;
- SQL Server.

A arquitetura continuará evoluindo sem alterar as responsabilidades das camadas Domain e Application.

**Status:** 🚧 Em desenvolvimento

---

# Evolução prevista

```text
Foundation
    │
    ▼
Domain
    │
    ▼
Domain Events
    │
    ▼
Application (CQRS)
    │
    ▼
Infrastructure
    │
    ▼
WebApi
    │
    ▼
RabbitMQ
    │
    ▼
Outbox
    │
    ▼
Inbox
    │
    ▼
Arquitetura Distribuída
    │
    ▼
Observabilidade
```

---

## Observações

A evolução do OrderFlow ocorre de forma incremental.

Cada capítulo consolida uma etapa da arquitetura antes da introdução de novos componentes, mantendo a documentação sincronizada com a implementação.