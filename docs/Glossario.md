# Glossário

Este documento reúne os principais termos utilizados durante o desenvolvimento do **OrderFlow**.

Os conceitos apresentados aqui possuem definições resumidas. Para um aprofundamento, consulte os documentos da pasta `docs/concepts`.

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

Exemplos:

- CreateOrder
- CancelOrder
- PayOrder

---

## CQRS (Command Query Responsibility Segregation)

Padrão arquitetural que separa operações de escrita (**Commands**) das operações de leitura (**Queries**).

---

# D

## Domain Event

Evento que representa algo importante que aconteceu dentro do domínio.

Exemplo:

- OrderCreatedDomainEvent

Posteriormente esses eventos poderão originar Integration Events.

---

## Domain Exception

Exceção utilizada para representar violações de regras de negócio.

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

Responsável por orquestrar um caso de uso da aplicação.

Não contém regras de negócio.

---

# I

## Invariante

Regra de negócio que deve permanecer verdadeira durante todo o ciclo de vida do Aggregate.

Exemplos:

- Um pedido deve possuir pelo menos um item.
- Um pedido cancelado não pode receber novos itens.
- O valor total do pedido deve corresponder à soma dos seus itens.

---

# P

## Pipeline Behavior

Componente do MediatR responsável por executar comportamentos transversais antes ou depois dos Handlers.

No OrderFlow, o primeiro comportamento implementado foi o `ValidationBehavior`.

---

# Q

## Query

Objeto responsável por representar uma operação de leitura.

Queries nunca alteram o estado do domínio.

---

# R

## Read Model

Modelo de dados utilizado exclusivamente para consultas.

No OrderFlow, as Queries retornam DTOs específicos de leitura, evitando expor diretamente as entidades do domínio.

---

## Read Repository

Repositório especializado em operações de leitura.

No OrderFlow, essa responsabilidade é representada pela interface `IOrderReadRepository`.

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

Essa abordagem aumenta a coesão e facilita a manutenção da aplicação.