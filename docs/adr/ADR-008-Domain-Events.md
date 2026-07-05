# ADR-008 — Domain Events

## Status

Aceita

---

## Contexto

O OrderFlow utilizará comunicação assíncrona baseada em eventos.

Era necessário definir como o domínio registraria acontecimentos importantes sem depender de tecnologias como RabbitMQ ou SQL Server.

---

## Decisão

As entidades registrarão Domain Events.

A publicação desses eventos será responsabilidade da camada Infrastructure.

As entidades não conhecem:

- RabbitMQ
- Outbox Pattern
- Dispatcher
- Mensageria

Elas apenas registram fatos de negócio.

---

## Decisões derivadas

- Domain Events pertencem ao Domain.
- Integration Events pertencerão à Infrastructure.
- A publicação ocorrerá após a persistência dos dados.
- A Entity armazenará os eventos temporariamente.

---

## Consequências

### Positivas

- Baixo acoplamento.
- Alta coesão.
- Independência tecnológica.
- Compatibilidade com Outbox.
- Facilidade para testes.

### Negativas

- Necessidade de Dispatcher.
- Infraestrutura mais sofisticada.

---

## Documentos relacionados

- Concepts/07-Domain-Events
- DEC-010-Por-que-Domain-Events