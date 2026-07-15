# Glossário

Este documento reúne os principais termos utilizados durante o desenvolvimento do **OrderFlow**.

Os conceitos apresentados aqui possuem definições resumidas. Para um aprofundamento, consulte os documentos disponíveis na pasta `docs/concepts`.

---

# A

## Aggregate

Conjunto de entidades e objetos de valor que devem permanecer consistentes como uma única unidade de negócio.

No OrderFlow, o Aggregate é composto por:

- Order (Aggregate Root)
- OrderItem

---

## Aggregate Root

Entidade responsável por controlar o acesso ao Aggregate e garantir a consistência de todas as suas regras de negócio.

No OrderFlow, a Aggregate Root é a entidade `Order`.

---

# AsNoTracking

Configuração utilizada em consultas do Entity Framework Core para evitar que os objetos retornados sejam armazenados no Change Tracker.

No OrderFlow, é utilizada pelo `OrderReadRepository` nas consultas destinadas apenas à leitura.

---

# Backing Field

Campo privado utilizado pelo Entity Framework Core para armazenar o estado de uma propriedade ou navegação sem exigir que a coleção mutável seja exposta publicamente.

No OrderFlow, o campo `_items` é utilizado como backing field da propriedade `Items` do Aggregate `Order`.

---

# C

## Command

Objeto que representa uma intenção de alterar o estado do sistema.

No OrderFlow:

- CreateOrder
- CancelOrder
- PayOrder

Os Commands executam regras de negócio através dos Aggregates e podem gerar Domain Events.

---

## CQRS (Command Query Responsibility Segregation)

Padrão arquitetural que separa operações de escrita (**Commands**) das operações de leitura (**Queries**), permitindo que ambas evoluam de forma independente.

---

# Change Tracking

Mecanismo do Entity Framework Core responsável por acompanhar o estado das entidades carregadas pelo `DbContext`.

No OrderFlow, os Aggregates são carregados de forma rastreada, alterados por comportamentos de domínio e persistidos através de `SaveChangesAsync()`.

---

# Cascade Delete

Comportamento de exclusão no qual os registros dependentes são removidos quando o registro principal é excluído fisicamente.

No OrderFlow, uma exclusão física de `Order` também remove seus `OrderItems`. O cancelamento de um pedido não utiliza exclusão física.

---

# D

## Domain Event

Evento que representa algo relevante ocorrido dentro do domínio.

Exemplo:

- OrderCreatedDomainEvent
- OrderCancelledDomainEvent
- OrderPaidDomainEvent

No futuro, esses eventos poderão originar Integration Events.

---

## Domain Exception

Exceção utilizada para representar violações das regras de negócio do domínio.

---

# DbContext

Classe central do Entity Framework Core responsável por representar uma sessão de trabalho com o banco de dados.

O `OrderFlowDbContext` gerencia o Change Tracking, os mapeamentos e a persistência das entidades do OrderFlow.

---

# E

## Entity

Objeto que possui identidade própria durante todo o seu ciclo de vida.

No OrderFlow:

- Order
- OrderItem

---

# Eager Loading

Estratégia de carregamento na qual entidades relacionadas são obtidas juntamente com a entidade principal.

No OrderFlow, o `OrderRepository` carrega o Aggregate `Order` com seus itens para garantir que o comportamento de domínio opere sobre o Aggregate completo.

---

# Entity Configuration

Classe responsável por definir o mapeamento de uma entidade através da Fluent API do Entity Framework Core.

No OrderFlow, `OrderConfiguration` e `OrderItemConfiguration` concentram os mapeamentos da persistência.

---

# Fluent API

API de configuração do Entity Framework Core utilizada para mapear entidades, propriedades, relacionamentos, índices e restrições sem adicionar atributos de persistência ao Domain.

---

# Foreign Key

Restrição que relaciona registros entre tabelas e preserva a integridade referencial.

No OrderFlow, `OrderItems.OrderId` referencia `Orders.Id`.

---

# H

## Handler

Componente responsável por orquestrar um caso de uso da aplicação.

O Handler:

- recebe um Command ou Query;
- coordena a execução;
- solicita persistência quando necessário;
- retorna o resultado.

Não implementa regras de negócio.

---

# I

## Invariante

Regra de negócio que deve permanecer verdadeira durante todo o ciclo de vida de um Aggregate.

Exemplos:

- Um pedido deve possuir pelo menos um item.
- Um pedido cancelado não pode receber novos itens.
- O valor total do pedido deve corresponder à soma dos seus itens.

---

# Migration

Arquivo gerado pelo Entity Framework Core que descreve as alterações necessárias para evoluir o esquema do banco de dados entre dois estados do modelo.

A migration inicial do OrderFlow é chamada `InitialCreate`.

---

# Model Snapshot

Arquivo mantido automaticamente pelo Entity Framework Core que representa o estado atual do modelo conhecido pelo framework.

Ele é utilizado como base de comparação para gerar novas migrations e não deve ser alterado manualmente.

---

# P

## Pipeline Behavior

Componente do MediatR responsável por executar comportamentos transversais antes ou depois da execução dos Handlers.

No OrderFlow, o primeiro comportamento implementado foi o `ValidationBehavior`.

---

# Q

## Query

Objeto responsável por representar uma operação de leitura.

As Queries:

- nunca alteram o estado do domínio;
- não executam regras de negócio;
- retornam modelos específicos de leitura (Read Models).

---

# R

## Read Model

Modelo de dados utilizado exclusivamente para consultas.

No OrderFlow, cada Query retorna um DTO específico de leitura, evitando expor diretamente as entidades do domínio.

---

## Read Repository

Repositório especializado em operações de leitura.

No OrderFlow, essa responsabilidade é representada pela interface:

```text
IOrderReadRepository
```

Essa separação reforça a aplicação do padrão CQRS.

---

## Repository

Abstração responsável pelo acesso aos Aggregates do domínio.

No OrderFlow, o `IOrderRepository` é utilizado exclusivamente pelos Commands para persistir alterações sobre o Aggregate `Order`.

---

# Shadow Property

Propriedade existente apenas no modelo interno do Entity Framework Core, sem estar declarada na classe de domínio.

No OrderFlow, `OrderId` é uma Shadow Property utilizada como Foreign Key de `OrderItem`.

---

# SQL Server

Banco de dados relacional utilizado pelo OrderFlow para persistir os Aggregates da aplicação.

No ambiente atual, o SQL Server está instalado localmente e é acessado através do Entity Framework Core.

# U

## Unit of Work

Abstração responsável por confirmar as alterações realizadas durante um caso de uso.

No OrderFlow, o `IUnitOfWork` desacopla a camada Application da implementação do Entity Framework Core.

---


# User Secrets

Mecanismo utilizado durante o desenvolvimento para armazenar configurações sensíveis fora do código-fonte.

No OrderFlow, a Connection String do SQL Server é armazenada em User Secrets.

---

# V

## ValidationBehavior

Pipeline Behavior responsável por executar automaticamente todos os Validators registrados antes da execução do Handler.

Caso exista alguma falha de validação, o Handler não é executado.

---

## Validator

Componente responsável por validar os dados de entrada de um Command ou Query.

No OrderFlow, os Validators são implementados utilizando **FluentValidation**.

Eles validam apenas a entrada da aplicação, enquanto as regras de negócio permanecem encapsuladas no domínio.

---

## Vertical Slice Architecture

Estratégia de organização em que cada caso de uso possui todos os seus componentes agrupados em uma única pasta.

Exemplo:

```text
CreateOrder
├── CreateOrderCommand.cs
├── CreateOrderCommandHandler.cs
├── CreateOrderCommandValidator.cs
└── CreateOrderResponse.cs
```

Cada funcionalidade é desenvolvida de forma independente, aumentando a coesão, reduzindo o acoplamento e facilitando a evolução da aplicação.