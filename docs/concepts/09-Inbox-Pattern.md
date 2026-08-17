# 10-Inbox-Pattern

Status: Aceita

---

# Objetivo

O **Inbox Pattern** é um padrão utilizado no lado consumidor de uma arquitetura orientada a eventos para impedir que uma mesma mensagem produza efeitos de negócio mais de uma vez.

Em sistemas que utilizam o modelo **At Least Once Delivery**, uma mensagem pode ser entregue novamente caso o broker não receba a confirmação de processamento.

O Inbox Pattern mantém um registro persistente das mensagens recebidas, permitindo que o consumidor determine se determinado evento já foi processado antes de executar novamente sua lógica.

---

# O Problema da Entrega Duplicada

Considere o seguinte fluxo:

```text
RabbitMQ
    ↓
Consumer
    ↓
Processa mensagem
    ↓
Executa regra de negócio
    ↓
Falha antes do ACK
    ↓
RabbitMQ
    ↓
Redelivery
    ↓
Consumer
    ↓
Executa novamente
```

Embora a lógica de negócio tenha sido executada, o RabbitMQ não recebeu o ACK.

Como consequência, a mensagem pode ser entregue novamente e produzir os mesmos efeitos de negócio mais de uma vez.

---

# At Least Once Delivery

O modelo **At Least Once Delivery** prioriza a confiabilidade da entrega.

Caso uma mensagem não seja confirmada, ela poderá ser entregue novamente ao consumidor.

Isso significa que:

> **At Least Once Delivery não significa Exactly Once Processing.**

A aplicação consumidora precisa estar preparada para receber mensagens duplicadas e impedir que elas produzam novamente os mesmos efeitos de negócio.

O **Inbox Pattern** fornece um mecanismo persistente para realizar esse controle.

---

# Como o Inbox Pattern Funciona

O Inbox Pattern introduz uma persistência no lado consumidor para registrar as mensagens recebidas e controlar seu processamento.

Cada evento publicado pelo OrderFlow possui um identificador único:

```text
EventId
    ↓
Domain Event
    ↓
OutboxMessages
    ↓
RabbitMQ
    ↓
Consumer
    ↓
Inbox
```

Ao receber uma mensagem, o Consumer utiliza o `EventId` para verificar se aquele evento já foi processado.

O fluxo conceitual é:

```text
Mensagem recebida
        ↓
     EventId
        ↓
Consultar Inbox
        ↓
Já processada?
   ┌────┴────┐
   │         │
  Sim       Não
   │         │
   ▼         ▼
Ignora    Processa
lógica     mensagem
   │         │
   │         ▼
   │     Marca como
   │     processada
   │         │
   └────┬────┘
        ▼
       ACK
```

A Inbox não impede que o RabbitMQ entregue novamente uma mensagem.

Sua responsabilidade é impedir que uma nova entrega produza novamente os mesmos efeitos de negócio.

---

# Primeira Entrega

Na primeira vez que determinado `EventId` é recebido, ainda não existe um processamento concluído correspondente na Inbox.

```text
RabbitMQ
    ↓
Consumer
    ↓
EventId não processado
    ↓
Processamento
    ↓
Inbox
    ↓
Marca como processado
    ↓
ACK
```

A mensagem executa normalmente sua lógica de negócio e, após a conclusão do processamento, seu estado é registrado na Inbox.

A partir desse momento, o `EventId` representa uma mensagem já processada.

---

# Redelivery

Caso o RabbitMQ entregue novamente uma mensagem cujo `EventId` já esteja marcado como processado:

```text
RabbitMQ
    ↓
Consumer
    ↓
EventId
    ↓
Inbox
    ↓
Já processado
    ↓
Não executa novamente
    ↓
ACK
```

O ACK informa ao RabbitMQ que a mensagem não precisa continuar sendo entregue.

Dessa forma, a aplicação aceita a possibilidade de **entrega duplicada**, mas evita **efeitos de negócio duplicados**.

> **Outbox protege o lado produtor; Inbox protege o lado consumidor.**

---

# Outbox × Inbox

O **Transactional Outbox Pattern** e o **Inbox Pattern** atuam em lados diferentes do fluxo de mensageria e resolvem problemas complementares.

O Outbox protege o **lado produtor**, garantindo que um evento gerado pelo domínio seja persistido antes de sua publicação no RabbitMQ.

O Inbox protege o **lado consumidor**, garantindo que uma mensagem entregue mais de uma vez não produza novamente os mesmos efeitos de negócio.

O fluxo completo pode ser representado da seguinte forma:

```text
Producer
   │
   ▼
Domain Event
   │
   ▼
OutboxMessages
   │
   ▼
Worker.Outbox
   │
   ▼
RabbitMQ
   │
   ▼
Consumer
   │
   ▼
Inbox
   │
   ▼
Processamento Idempotente
   │
   ▼
ACK
```

As responsabilidades podem ser resumidas da seguinte forma:

| Padrão               | Lado       | Responsabilidade                                                        |
| -------------------- | ---------- | ----------------------------------------------------------------------- |
| Transactional Outbox | Produtor   | Garantir que eventos não sejam perdidos entre banco de dados e broker   |
| Inbox                | Consumidor | Impedir que mensagens duplicadas produzam efeitos de negócio duplicados |

Dessa forma, os dois padrões trabalham em conjunto:

```text
Outbox
   ↓
Publicação confiável
   ↓
RabbitMQ
   ↓
Entrega At Least Once
   ↓
Inbox
   ↓
Processamento idempotente
```

> **Outbox garante publicação confiável; Inbox garante processamento idempotente.**

---

# Inbox não é Exactly Once Delivery

A utilização do Inbox Pattern não transforma o sistema em um mecanismo de **Exactly Once Delivery**.

O RabbitMQ ainda pode entregar a mesma mensagem mais de uma vez.

O que muda é o comportamento do consumidor diante dessas entregas.

```text
Mensagem
   ↓
Entrega 1 ──→ Processamento
   ↓
Entrega 2 ──→ Detectada como duplicada
   ↓
Entrega 3 ──→ Detectada como duplicada
```

Portanto, o Inbox Pattern não garante que a mensagem será entregue apenas uma vez.

Ele garante que, uma vez reconhecido o processamento concluído de determinado `EventId`, novas entregas possam ser identificadas antes de repetir seus efeitos de negócio.

---

# EventId

O `EventId` é o elemento central para a identificação das mensagens.

Cada Domain Event recebe um identificador único no momento de sua criação.

Esse identificador é preservado durante todo o fluxo:

```text
Domain Event
    ↓
EventId
    ↓
OutboxMessages
    ↓
RabbitMQ
    ↓
Consumer
    ↓
Inbox
```

Quando o Consumer recebe uma mensagem, o `EventId` permite determinar se aquela mensagem corresponde a um evento novo ou a uma nova entrega de um evento já processado.

Por isso, o identificador deve permanecer estável durante todo o ciclo de vida do evento.

---

# Persistência da Inbox

A Inbox precisa utilizar armazenamento persistente.

Uma implementação somente em memória perderia o histórico de processamento quando o Worker fosse reiniciado.

```text
Worker
   ↓
Memória
   ↓
Restart
   ↓
Histórico perdido
```

Com persistência:

```text
Worker
   ↓
SQL Server
   ↓
Restart
   ↓
SQL Server
   ↓
Histórico preservado
```

No OrderFlow, a Inbox será persistida no **SQL Server**, aproveitando a infraestrutura de persistência já existente na solução.

Isso permite que o controle de duplicidade sobreviva a:

* reinicializações do Worker;
* indisponibilidades temporárias;
* redeliveries;
* execução de múltiplas instâncias de Consumers.

---

# Inbox e Retry

Inbox e Retry resolvem problemas diferentes.

O **Retry** permite tentar novamente o processamento de uma mensagem que apresentou uma falha transitória.

O **Inbox** impede que uma mensagem cujo processamento já foi concluído execute novamente seus efeitos de negócio.

```text
Mensagem
    ↓
Processamento
    ↓
Falhou?
 ┌──┴───┐
 │      │
Sim    Não
 │      │
 ▼      ▼
Retry  Inbox
        ↓
   Processada
```

Uma mensagem cujo processamento falhou não deve ser considerada concluída apenas porque foi recebida.

Ela precisa continuar elegível para uma nova tentativa.

---

# Inbox e Dead Letter Queue

Quando uma mensagem apresenta uma falha permanente ou excede a política de Retry, ela poderá ser direcionada para a **Dead Letter Queue**.

```text
Mensagem
    ↓
Consumer
    ↓
Falha
    ↓
Retry
    ↓
Falha novamente
    ↓
DLQ
```

O Inbox Pattern não substitui a DLQ.

Cada mecanismo possui uma responsabilidade diferente:

| Mecanismo | Responsabilidade                                 |
| --------- | ------------------------------------------------ |
| Inbox     | Evitar efeitos duplicados                        |
| Retry     | Recuperar falhas transitórias                    |
| DLQ       | Isolar mensagens que não puderam ser processadas |

Esses mecanismos trabalham em conjunto para aumentar a confiabilidade do processamento assíncrono.

---

# Idempotência

Idempotência significa que repetir determinada operação não deve produzir efeitos adicionais indesejados.

No contexto do Inbox Pattern:

```text
Evento X
    ↓
Primeira entrega
    ↓
Executa efeito

Evento X
    ↓
Redelivery
    ↓
Não executa efeito novamente
```

A Inbox fornece a infraestrutura necessária para identificar o processamento anterior da mensagem.

Isso permite que o Consumer implemente comportamento idempotente mesmo utilizando um broker baseado em **At Least Once Delivery**.

---

# Fluxo Completo no OrderFlow

Com Outbox e Inbox, o fluxo conceitual do OrderFlow passa a ser:

```text
Order
    ↓
Domain Event
    ↓
OutboxMessages
    ↓
Commit SQL Server
    ↓
Worker.Outbox
    ↓
RabbitMQ
    ↓
Worker.Payments
    ↓
EventId
    ↓
Inbox
    ↓
Já processado?
 ┌─────┴─────┐
 │           │
Sim         Não
 │           │
 ▼           ▼
ACK      Processamento
             ↓
        Marca processado
             ↓
            ACK
```

Esse fluxo combina:

* persistência transacional no produtor;
* publicação assíncrona;
* entrega **At Least Once**;
* deduplicação persistente;
* processamento idempotente;
* Retry;
* Dead Letter Queue.

---

# Resumo

O Inbox Pattern complementa o Transactional Outbox no lado consumidor.

Enquanto o Outbox garante que eventos persistidos pelo produtor possam ser publicados de forma confiável, o Inbox permite que o consumidor reconheça mensagens já processadas.

Os principais conceitos são:

* mensagens podem ser entregues mais de uma vez;
* cada evento possui um `EventId` único e estável;
* a Inbox mantém um registro persistente do processamento;
* mensagens já concluídas não repetem seus efeitos de negócio;
* mensagens duplicadas podem ser confirmadas com ACK;
* mensagens que falharam permanecem elegíveis para nova tentativa;
* Inbox, Retry e DLQ possuem responsabilidades distintas;
* At Least Once Delivery exige consumidores preparados para duplicidades.

> **Transactional Outbox garante publicação confiável. Inbox Pattern garante processamento idempotente.**
