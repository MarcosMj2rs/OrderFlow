# ADR-001 - Princípios Arquiteturais

**Status:** Aceita

## Contexto

O projeto **OrderFlow** foi criado com o objetivo de servir como um laboratório de estudo sobre desenvolvimento de aplicações distribuídas utilizando .NET, Domain-Driven Design (DDD), mensageria com RabbitMQ e padrões de arquitetura modernos.

Para garantir consistência técnica e facilitar a evolução da solução, é necessário definir um conjunto de princípios arquiteturais que orientarão todas as decisões futuras do projeto.

Este ADR estabelece esses princípios como base para os demais documentos arquiteturais.

---

## Decisão

O OrderFlow adotará os seguintes princípios arquiteturais:

### 1. Clean Architecture

A solução será organizada em camadas independentes, respeitando a Regra da Dependência (Dependency Rule).

As dependências sempre apontam para o centro da aplicação, garantindo baixo acoplamento entre as camadas.

```text
Api
    ↓
Application
    ↓
Domain

Infrastructure
    ↑
```

O domínio não conhecerá detalhes de infraestrutura, frameworks ou tecnologias externas.

---

### 2. Domain-Driven Design (DDD)

O domínio representa o núcleo da aplicação.

As regras de negócio serão implementadas dentro das entidades e agregados, evitando modelos anêmicos.

Os principais conceitos adotados incluem:

- Aggregate Root
- Entity
- Value Object (quando necessário)
- Domain Event
- Repository
- Domain Services (quando apropriado)

---

### 3. CQRS

Operações de escrita e leitura serão separadas.

- Commands modificam estado.
- Queries apenas consultam dados.

Essa separação reduz acoplamento e facilita a evolução da aplicação.

---

### 4. Baixo Acoplamento

As camadas se comunicarão por meio de abstrações.

A Application dependerá apenas de interfaces, nunca de implementações concretas.

Exemplo:

```
Application
        ↓
IOrderRepository

Infrastructure
        ↓
OrderRepository
```

---

### 5. Alta Coesão

Cada classe deve possuir uma única responsabilidade.

Exemplos:

- Controller → HTTP
- Handler → Caso de uso
- Repository → Persistência
- Aggregate → Regras de negócio

---

### 6. Inversão de Dependência

Dependências externas serão injetadas utilizando Dependency Injection.

A composição da aplicação ocorrerá exclusivamente na camada de entrada (Composition Root).

---

### 7. Persistência Transparente

O domínio não conhecerá EF Core.

Toda persistência será realizada através de Repositories e Unit of Work.

---

### 8. Mensageria Orientada a Eventos

Mudanças relevantes de estado produzirão Domain Events.

Esses eventos poderão ser publicados para sistemas externos através do RabbitMQ.

O domínio não conhecerá RabbitMQ.

---

### 9. Observabilidade

A solução será preparada para oferecer:

- Logs estruturados
- Correlação de requisições
- Rastreamento de eventos
- Métricas

A observabilidade será considerada parte da arquitetura desde o início do projeto.

---

### 10. Evolução Incremental

A arquitetura será construída de forma incremental.

Cada etapa deverá produzir uma solução funcional antes da introdução de novos conceitos.

A ordem de evolução será:

1. Domain
2. Application
3. Infrastructure
4. Web API
5. RabbitMQ
6. Workers
7. Outbox
8. Inbox
9. Observabilidade

---

## Consequências

A adoção destes princípios proporciona:

### Positivas

- Baixo acoplamento.
- Alta testabilidade.
- Independência de frameworks.
- Facilidade de manutenção.
- Evolução incremental da solução.
- Clareza na separação de responsabilidades.
- Arquitetura adequada para aplicações distribuídas.

### Negativas

- Maior quantidade de projetos e abstrações.
- Curva de aprendizado superior à de aplicações em camadas tradicionais.
- Maior número de componentes para implementar inicialmente.

Entretanto, os benefícios superam os custos para um projeto cujo objetivo é estudar arquitetura de software moderna.

---

## ADRs Relacionados

- ADR-002 — Domain-Driven Design
- ADR-003 — Clean Architecture
- ADR-004 — CQRS
- ADR-005 — Entity Framework Core
- ADR-006 — SQL Server
- ADR-007 — RabbitMQ