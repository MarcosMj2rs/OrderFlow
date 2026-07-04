# ADR-003 - Domain-Driven Design

## Status

Aceita

---

## Contexto

O projeto OrderFlow possui como objetivo estudar arquiteturas distribuídas utilizando RabbitMQ.

Para isso, o domínio deve permanecer independente das tecnologias de infraestrutura.

---

## Decisão

Adotar Domain-Driven Design como modelo arquitetural para construção do domínio.

---

## Decisões derivadas

- Order será o Aggregate Root.
- OrderItem será uma Entity interna do Aggregate.
- OrderItem não possuirá Repository próprio.
- Apenas Order poderá modificar os itens.
- As coleções serão protegidas utilizando IReadOnlyCollection.
- O domínio utilizará DomainException para representar violações de regras de negócio.

---

## Consequências

### Positivas

- Maior encapsulamento.
- Regras centralizadas.
- Facilidade para testes.
- Menor acoplamento.

### Negativas

- Maior quantidade de código.
- Curva de aprendizado maior.