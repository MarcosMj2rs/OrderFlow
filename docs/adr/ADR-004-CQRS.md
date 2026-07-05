# ADR-004 — CQRS

## Status

Aceita

---

## Contexto

Era necessário definir uma estratégia para organizar os casos de uso da aplicação mantendo baixo acoplamento entre as responsabilidades.

---

## Decisão

Adotar CQRS utilizando MediatR.

Cada caso de uso será composto por:

- Command ou Query;
- Handler;
- Validator;
- Request/Response (quando necessário).

Todos os casos de uso serão organizados por Feature.

---

## Consequências

### Positivas

- Organização por caso de uso.
- Melhor testabilidade.
- Facilidade para evolução.
- Baixo acoplamento.

### Negativas

- Maior número de arquivos.

---

## Documentos relacionados

- Concepts/03-CQRS
- DEC-003-Por-que-CQRS

---

## Evolução

Após a implementação inicial, foram adicionados os seguintes componentes:

- ValidationBehavior
- IUnitOfWork
- CreateOrder
- CancelOrder

Todos os casos de uso permanecem organizados por Feature.

Cada Handler possui responsabilidade única de orquestração.