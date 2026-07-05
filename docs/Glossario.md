# Glossário

Este documento reúne os principais conceitos utilizados durante o desenvolvimento do OrderFlow.

---

## Aggregate

Conjunto de entidades e objetos de valor que devem permanecer consistentes como uma única unidade de negócio.

No OrderFlow, o Aggregate é composto por:

- Order (Aggregate Root)
- OrderItem (Entity)

---

## Aggregate Root

Entidade responsável por controlar o acesso e garantir a consistência de todo o Aggregate.

No OrderFlow, a Aggregate Root é a entidade `Order`.

---

## Entity

Objeto que possui identidade própria durante todo seu ciclo de vida.

No OrderFlow:

- Order
- OrderItem

---

## Invariante

Regra de negócio que deve permanecer verdadeira durante todo o ciclo de vida do Aggregate.

Exemplos:

- Um pedido deve possuir pelo menos um item.
- Um pedido cancelado não pode receber novos itens.
- O valor total do pedido deve ser igual à soma dos itens.

---

## Domain Exception

Exceção utilizada para representar violações de regras de negócio do domínio.

## CQRS

Padrão arquitetural que separa operações de escrita (Commands) das operações de leitura (Queries).

---

## Command

Representa uma intenção de alterar o estado do sistema.

---

## Query

Representa uma consulta.

Nunca altera o estado do domínio.

---

## Handler

Responsável por orquestrar um caso de uso.

Não contém regras de negócio.

---

## Pipeline Behavior

Componente do MediatR responsável por executar comportamentos transversais antes ou depois dos Handlers.

Exemplo:

- ValidationBehavior