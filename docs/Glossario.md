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

# E

## Entity

Objeto que possui identidade própria durante todo o seu ciclo de vida.

No OrderFlow:

- Order
- OrderItem

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

# U

## Unit of Work

Abstração responsável por confirmar as alterações realizadas durante um caso de uso.

No OrderFlow, o `IUnitOfWork` desacopla a camada Application da implementação do Entity Framework Core.

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