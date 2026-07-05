# DEC-003 — Por que CQRS?

## Problema

Como organizar os casos de uso da aplicação sem criar Services enormes e difíceis de manter?

---

## Alternativa 1

Application Services tradicionais.

### Vantagens

- Menos arquivos.

### Desvantagens

- Alto acoplamento.
- Classes grandes.
- Mistura de responsabilidades.

---

## Alternativa 2

CQRS utilizando MediatR.

### Vantagens

- Casos de uso independentes.
- Melhor manutenção.
- Melhor testabilidade.
- Integração natural com Pipeline Behaviors.

### Desvantagens

- Mais arquivos.
- Estrutura inicial mais complexa.

---

## Decisão

Foi adotado CQRS utilizando MediatR.

Cada caso de uso será tratado individualmente.

A lógica de negócio continuará centralizada no Domain.