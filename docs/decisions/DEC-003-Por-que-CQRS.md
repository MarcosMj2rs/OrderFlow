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

---

## Decisões complementares

Durante a implementação foram adotadas as seguintes decisões arquiteturais.

### ValidationBehavior

A validação ocorre antes da execução dos Handlers.

Dessa forma, os Handlers podem assumir que os Commands recebidos são válidos.

---

### Unit of Work

Foi criada uma abstração própria para ocultar detalhes do Entity Framework Core.

A camada Application não conhece DbContext.

---

### Repositories

Os Handlers dependem apenas de interfaces.

Nenhum Handler possui dependência direta de infraestrutura.