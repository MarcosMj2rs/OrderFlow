# ADR-009 - Outbox Pattern

**Status:** Aceita

## Registro de Decisões

| ID | Decisão |
|---|---|
| D01 | O OrderFlow utilizará o Transactional Outbox Pattern para garantir a publicação confiável de eventos de domínio. |
| D02 | Os Domain Events serão persistidos na mesma transação da entidade de negócio. |
| D03 | A publicação no RabbitMQ ocorrerá de forma assíncrona por um Worker dedicado. |
| D04 | Eventos publicados com sucesso serão marcados como processados, preservando o histórico. |
| D05 | O Outbox será implementado utilizando SQL Server. |
| D06 | A publicação utilizará o RabbitMQ já existente na solução. |
| D07 | O Outbox será integrado ao Unit of Work da aplicação. |
| D08 | O Worker.Outbox será responsável exclusivamente pela publicação dos eventos pendentes. |
| D09 | A estratégia deverá ser compatível com Retry, Dead Letter Queue e futura implementação do Inbox Pattern. |

## Escopo desta ADR

Esta ADR define a estratégia utilizada para garantir a publicação confiável de eventos gerados pelos agregados do domínio.

Estão contemplados:

- persistência transacional dos eventos;
- armazenamento em Outbox;
- publicação assíncrona;
- responsabilidades do Worker.Outbox;
- integração com RabbitMQ.

Não fazem parte desta ADR:

- Retry;
- Dead Letter Queue;
- Inbox Pattern;
- Idempotência do consumidor;
- Observabilidade;
- Monitoramento.

> **Categoria:** Messaging / Reliability
>
> **Relacionadas:**
>
> - ADR-007 — RabbitMQ
> - ADR-012 — Retry
> - ADR-013 — Dead Letter Queue

---

# Contexto

Atualmente, quando um pedido é criado, o fluxo ocorre da seguinte forma:

```text
Order
    ↓
SaveChanges()
    ↓
Commit
    ↓
RabbitMQ
```

Caso o banco de dados confirme a transação e o RabbitMQ fique indisponível imediatamente após o commit, o pedido será persistido, porém o evento será perdido.

Essa situação é conhecida como **Dual Write Problem**, pois duas operações independentes precisam ocorrer com sucesso:

- persistência no banco;
- publicação da mensagem.

Sem coordenação entre elas, existe risco de inconsistência.

---

# Problema

A publicação direta para o RabbitMQ após o commit não oferece garantia de entrega.

Exemplos:

- indisponibilidade temporária do RabbitMQ;
- falha de rede;
- reinício da aplicação;
- encerramento inesperado do processo.

Nessas situações o banco permanece consistente, mas os consumidores jamais receberão o evento.

---

# Drivers Arquiteturais

## Confiabilidade

Nenhum evento de domínio deve ser perdido.

---

## Consistência

A gravação da entidade e do evento devem ocorrer na mesma transação.

---

## Recuperação

Caso o RabbitMQ esteja indisponível, o evento deverá permanecer armazenado até nova tentativa.

---

## Baixo Acoplamento

A camada Domain não conhecerá detalhes de publicação.

A Application continuará dependente apenas de abstrações.

---

# Alternativas Consideradas

## Publicação imediata após SaveChanges

**Vantagens**

- implementação simples.

**Desvantagens**

- risco de perda de mensagens;
- Dual Write Problem.

**Decisão:** Rejeitada.

---

## Transações distribuídas (2PC)

**Vantagens**

- consistência forte.

**Desvantagens**

- alta complexidade;
- baixo desempenho;
- dependência de infraestrutura específica.

**Decisão:** Rejeitada.

---

## Transactional Outbox

Funcionamento:

```text
Transaction

Orders
OutboxMessages

Commit

↓

Worker.Outbox

↓

RabbitMQ
```

**Vantagens**

- elimina perda de eventos;
- desacopla persistência da publicação;
- compatível com Event-Driven Architecture;
- amplamente utilizado em sistemas distribuídos.

**Desvantagens**

- aumenta a complexidade;
- exige Worker adicional;
- requer limpeza periódica da tabela.

**Decisão:** Aceita.

---

# Decisão

O OrderFlow utilizará o **Transactional Outbox Pattern**.

Todo Domain Event será persistido na tabela **OutboxMessages** durante a mesma transação utilizada para persistir o agregado.

Após o commit, um Worker dedicado consultará os eventos pendentes e realizará sua publicação no RabbitMQ.

---

# Fluxo Arquitetural

```mermaid
flowchart LR

A[Command Handler]

--> B[Aggregate Root]

--> C[Domain Events]

--> D[UnitOfWork]

--> E[SQL Server]

E --> F[Orders]

E --> G[OutboxMessages]

G --> H[Worker.Outbox]

H --> I[RabbitMqEventPublisher]

I --> J[RabbitMQ]
```

---

# Responsabilidades

## Domain

Gerar Domain Events.

---

## Application

Executar casos de uso.

---

## UnitOfWork

Persistir:

- entidades;
- eventos da Outbox.

---

## Worker.Outbox

Responsável por:

- localizar eventos pendentes;
- publicar no RabbitMQ;
- marcar como processados.

---

## RabbitMQ

Distribuir os eventos para os consumidores.

---

# Consequências Positivas

- elimina perda de eventos;
- publicação resiliente;
- recuperação automática;
- desacoplamento entre persistência e mensageria.

---

# Consequências Negativas

- necessidade de Worker adicional;
- tabela extra;
- estratégia de limpeza futura.

---

# Trade-offs

| Decisão | Benefício | Custo |
|---|---|---|
| Outbox | Confiabilidade | Mais infraestrutura |
| Worker dedicado | Desacoplamento | Mais processamento |
| Persistência dos eventos | Recuperação | Mais armazenamento |

---

# Impacto nas Camadas

## Domain

Nenhum.

---

## Application

Integração com UnitOfWork.

---

## Infrastructure

Implementação da tabela Outbox, Worker e publicação.

---

## Worker.Outbox

Novo componente responsável pela publicação dos eventos.

---

# Critérios de Validação

A implementação será considerada concluída quando:

- pedidos e eventos forem persistidos na mesma transação;
- eventos permanecerem na Outbox caso o RabbitMQ esteja indisponível;
- Worker.Outbox publicar eventos pendentes;
- eventos publicados forem marcados como processados;
- nenhuma mensagem seja perdida após reinicialização da aplicação.

---

# Referências

- Enterprise Integration Patterns
- Microservices Patterns — Chris Richardson
- Transactional Outbox Pattern
- Microsoft Architecture Guides

---

# Histórico

| Data | Alteração |
|---|---|
| 04/08/2026 | Criação da ADR-009 definindo a estratégia de Transactional Outbox do OrderFlow. |