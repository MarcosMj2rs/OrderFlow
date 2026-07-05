# Domain Events

## 1. Objetivo

Apresentar o conceito de Domain Events e sua aplicação no projeto OrderFlow.

Os Domain Events permitem que o domínio registre fatos relevantes de negócio sem conhecer qualquer tecnologia de infraestrutura, mantendo o baixo acoplamento entre as camadas da aplicação.

---

## 2. Motivação

Em arquiteturas distribuídas, diversas ações podem ocorrer após uma mudança de estado do domínio.

Exemplos:

- Enviar um e-mail;
- Publicar uma mensagem no RabbitMQ;
- Atualizar outro microsserviço;
- Gravar uma Outbox;
- Atualizar métricas.

Se a entidade executasse todas essas ações diretamente, ela passaria a conhecer detalhes da infraestrutura, violando os princípios da Clean Architecture e do DDD.

Os Domain Events resolvem esse problema.

---

## 3. Conceito

Um Domain Event representa um fato de negócio que já ocorreu.

Exemplos:

- Pedido criado;
- Pedido pago;
- Pedido cancelado.

Um Domain Event nunca representa uma intenção.

| Command | Domain Event |
|----------|--------------|
| PayOrder | OrderPaid |

Command significa:

> Quero pagar o pedido.

Domain Event significa:

> O pedido foi pago.

---

## 4. Como aplicamos no OrderFlow

O domínio registra eventos diretamente na classe base `Entity`.

Cada entidade possui uma coleção interna de Domain Events.

Quando uma regra de negócio é executada com sucesso, a entidade registra um evento utilizando:

```csharp
RaiseDomainEvent(...)
```

Nenhuma entidade conhece:

- RabbitMQ
- SQL Server
- Outbox Pattern
- Infrastructure

Ela apenas registra que algo aconteceu.

---

## 5. Fluxo

```text
Application

↓

Order.Pay()

↓

Status = Paid

↓

RaiseDomainEvent(
    OrderPaidDomainEvent)

↓

Entity.DomainEvents
```

Posteriormente:

```text
DbContext

↓

Dispatcher

↓

Outbox

↓

RabbitMQ
```

---

## 6. Eventos implementados

Atualmente o OrderFlow possui:

- OrderCreatedDomainEvent
- OrderPaidDomainEvent
- OrderCancelledDomainEvent

---

## 7. Benefícios

- Baixo acoplamento.
- Alta coesão.
- Independência da infraestrutura.
- Preparação para Outbox Pattern.
- Facilidade para testes unitários.
- Preparação para arquiteturas distribuídas.

---

## 8. Desvantagens

- Necessidade de um mecanismo para publicação.
- Maior quantidade de infraestrutura.
- Fluxo um pouco mais complexo.

---

## 9. Boas práticas

- Registrar apenas fatos relevantes.
- Nunca publicar mensagens diretamente pela entidade.
- Manter Domain Events independentes da infraestrutura.
- Utilizar nomes no passado.

Exemplos:

- OrderCreated
- OrderPaid
- OrderCancelled

---

## 10. Erros comuns

❌ Publicar RabbitMQ dentro da entidade.

❌ Chamar serviços externos pela entidade.

❌ Utilizar Domain Events como Commands.

❌ Colocar lógica de infraestrutura dentro do Domain.

---

## 11. Conclusão

Os Domain Events permitem que o domínio informe acontecimentos importantes sem conhecer quem irá consumi-los.

Essa decisão torna o domínio completamente desacoplado da infraestrutura e prepara o projeto para utilização de Outbox Pattern e RabbitMQ.

---

## 12. Documentos relacionados

- ADR-008-Domain-Events
- DEC-010-Por-que-Domain-Events
- 08-Outbox-Pattern
- 09-Inbox-Pattern