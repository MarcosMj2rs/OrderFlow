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

- OrderCreatedDomainEvent;
- OrderCancelledDomainEvent;
- OrderPaidDomainEvent.

Também foi criada a infraestrutura para gerenciamento de Domain Events através da classe base `Entity`.

**Status:** ✅ Concluído

---

# Capítulo 4 — Application (CQRS)

Foi construída toda a camada **Application** utilizando CQRS.

Principais componentes implementados:

## Commands

- CreateOrder;
- CancelOrder;
- PayOrder.

## Queries

- GetOrderById;
- GetOrders.

## Behaviors

- ValidationBehavior.

## Abstrações

- IUnitOfWork;
- IOrderRepository;
- IOrderReadRepository.

## Tecnologias

- MediatR;
- FluentValidation;
- Vertical Slice Architecture.

Ao término deste capítulo, a camada **Application** encontra-se completamente implementada e desacoplada da infraestrutura.

**Status:** ✅ Concluído

---

# Capítulo 5 — Infrastructure

Foi implementada toda a camada **Infrastructure**, concretizando as abstrações definidas na Application.

Principais componentes implementados:

## Entity Framework Core

- OrderFlowDbContext;
- Fluent API;
- Entity Configurations;
- Change Tracking;
- Backing Fields;
- Shadow Properties.

## Persistência

- OrderRepository;
- OrderReadRepository;
- UnitOfWork.

## SQL Server

- integração com SQL Server local;
- configuração via Dependency Injection;
- Connection String utilizando User Secrets.

## Banco de Dados

- primeira migration (`InitialCreate`);
- criação automática do banco;
- criação das tabelas;
- criação dos índices;
- criação das Foreign Keys;
- utilização do Model Snapshot para controle da evolução do modelo.

## Conceitos estudados

- DbContext;
- Fluent API;
- Migrations;
- Model Snapshot;
- AsNoTracking;
- Change Tracking;
- Aggregate Mapping;
- Backing Fields;
- Shadow Properties;
- User Secrets.

Ao término deste capítulo, toda a infraestrutura de persistência encontra-se implementada e integrada ao SQL Server.

**Status:** ✅ Concluído

---

# Próxima etapa — WebApi

O próximo capítulo será responsável por expor os casos de uso da aplicação através de uma API REST.

Serão desenvolvidos:

- Controllers;
- Endpoints;
- Swagger/OpenAPI;
- Pipeline HTTP;
- Integração com MediatR;
- Testes de integração dos endpoints.

A camada WebApi permanecerá responsável apenas pela orquestração das requisições, mantendo as regras de negócio concentradas nas camadas Domain e Application.

**Status:** ⏳ Planejado

---

# Etapas Futuras

Após a conclusão da WebApi, a arquitetura continuará evoluindo com foco em mensageria e sistemas distribuídos.

Próximas implementações previstas:

- RabbitMQ;
- Publisher;
- Consumer;
- Background Workers;
- Outbox Pattern;
- Inbox Pattern;
- Idempotência;
- Retry;
- Dead Letter Queue (DLQ);
- Saga;
- OpenTelemetry;
- Observabilidade;
- Testcontainers.

Cada etapa será construída sobre a infraestrutura já consolidada.

---

# Evolução da Arquitetura

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

Cada capítulo consolida completamente uma etapa da arquitetura antes da introdução de novos componentes, garantindo que:

- o código permaneça consistente;
- a documentação acompanhe a implementação;
- as decisões arquiteturais sejam registradas;
- os conceitos sejam compreendidos antes da evolução para a etapa seguinte.

Essa abordagem permite que o projeto funcione não apenas como uma aplicação de referência, mas também como um laboratório de estudo sobre Clean Architecture, DDD, CQRS, Entity Framework Core, SQL Server e, nas próximas etapas, mensageria e arquitetura distribuída.