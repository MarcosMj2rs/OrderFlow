# Por que utilizar Domain-Driven Design?

## Alternativas avaliadas

### Modelo Anêmico

Vantagens

- Simples.
- Pouco código.

Desvantagens

- Regras espalhadas.
- Baixo encapsulamento.
- Dificulta evolução.

---

### Domain-Driven Design

Vantagens

- Regras centralizadas.
- Aggregate consistente.
- Alta coesão.
- Melhor aderência a sistemas distribuídos.

Desvantagens

- Curva de aprendizado.
- Maior quantidade de código.

---

## Decisão

Foi adotado DDD por favorecer um modelo rico de domínio, requisito importante para a implementação futura de Domain Events, Outbox, Inbox e Idempotência.