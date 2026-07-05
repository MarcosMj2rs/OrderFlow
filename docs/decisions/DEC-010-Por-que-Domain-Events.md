# DEC-010 — Por que Domain Events?

## Problema

Como permitir que o domínio informe acontecimentos importantes sem conhecer a infraestrutura?

---

## Alternativas avaliadas

### Alternativa 1

A própria entidade publica mensagens.

```
Order

↓

RabbitMQ
```

#### Vantagens

- Implementação simples.

#### Desvantagens

- Forte acoplamento.
- Viola Clean Architecture.
- Difícil manutenção.
- Difícil teste.

---

### Alternativa 2

A entidade registra Domain Events.

Posteriormente outro componente publica os eventos.

```
Order

↓

Domain Event

↓

Dispatcher

↓

RabbitMQ
```

#### Vantagens

- Domínio desacoplado.
- Alta coesão.
- Compatível com Outbox.
- Compatível com RabbitMQ.
- Melhor testabilidade.

#### Desvantagens

- Exige infraestrutura adicional.

---

## Decisão

Foi adotada a Alternativa 2.

O domínio apenas registra acontecimentos.

A publicação será responsabilidade da Infrastructure.

---

## Impacto

Essa decisão permitirá implementar posteriormente:

- Outbox Pattern;
- RabbitMQ;
- Eventual Consistency;
- Idempotência;
- Retry;
- DLQ.