# 09-Inbox-Pattern

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
    Estado atual?
        │
        ├── PROCESSED
        │       ↓
        │   Não processa novamente
        │       ↓
        │      ACK
        │
        ├── PROCESSING
        │       ↓
        │   Timeout expirou?
        │       ├── Não → Já está sendo processada
        │       └── Sim → Reinicia processamento
        │
        ├── FAILED
        │       ↓
        │   Reinicia processamento
        │
        └── Não existe
                ↓
           Cria PROCESSING
                ↓
          Persiste o claim
                ↓
          Executa o handler
                ↓
          ┌─────┴─────┐
          │           │
       Sucesso       Falha
          │           │
          ▼           ▼
      PROCESSED     FAILED
          │
          ▼
         ACK
```

A Inbox não impede que o RabbitMQ entregue novamente uma mensagem. Sua responsabilidade é controlar de forma persistente o estado do processamento e impedir que uma nova entrega produza novamente os mesmos efeitos de negócio.

No OrderFlow, uma mensagem pode assumir os estados `PROCESSING`, `PROCESSED` ou `FAILED`. Antes da execução do handler, o Consumer realiza o **claim** da mensagem, persistindo o estado `PROCESSING`. Somente após esse claim ser confirmado o processamento da lógica de negócio é iniciado.

Essa separação permite que outras instâncias do Consumer identifiquem que uma mensagem já está sendo processada, além de possibilitar a recuperação de processamentos interrompidos por meio do timeout configurado.

---

# Primeira Entrega

Na primeira vez que determinado `EventId` é recebido, ainda não existe um registro correspondente na Inbox.

O Consumer realiza então o **claim** da mensagem, criando o registro com estado `PROCESSING` antes da execução do handler.

```text
RabbitMQ
    ↓
Consumer
    ↓
EventId não existe na Inbox
    ↓
Cria registro PROCESSING
    ↓
Persiste e confirma o claim
    ↓
Executa o handler
    ↓
Processamento concluído
    ↓
Marca como PROCESSED
    ↓
ACK
```

A mensagem executa normalmente sua lógica de negócio e, após a conclusão do processamento, seu estado é registrado na Inbox.

A partir desse momento, o `EventId` representa uma mensagem já processada.

---

# Redelivery

Quando o RabbitMQ entrega novamente uma mensagem, o comportamento depende do estado persistido para aquele `EventId` na Inbox.

```text
RabbitMQ
    ↓
Consumer
    ↓
EventId
    ↓
Consultar Inbox
    ↓
Estado?
    │
    ├── PROCESSED
    │       ↓
    │   Não executa novamente
    │       ↓
    │      ACK
    │
    ├── PROCESSING
    │       ↓
    │   Verifica timeout
    │
    └── FAILED
            ↓
        Nova tentativa
```

Uma mensagem em `PROCESSED` já concluiu seu processamento e, portanto, não executará novamente o handler.

Uma mensagem em `FAILED` permanece elegível para uma nova tentativa de processamento.

Quando a mensagem estiver em `PROCESSING`, o Consumer verificará o tempo decorrido desde o início do processamento. Se o timeout ainda não tiver expirado, outra instância não deverá executar simultaneamente o handler. Se o timeout tiver expirado, o processamento poderá ser reiniciado.

Dessa forma, a aplicação aceita a possibilidade de **entrega duplicada**, mas controla persistentemente se aquela entrega deve ou não executar novamente a lógica de negócio.

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

## Estado Persistido

Além do `EventId`, a Inbox mantém informações que permitem acompanhar o ciclo de vida do processamento da mensagem.

Os principais estados são:

| Estado | Significado |
| --- | --- |
| `PROCESSING` | A mensagem foi reivindicada por um Consumer e seu processamento foi iniciado. |
| `PROCESSED` | O processamento foi concluído com sucesso. |
| `FAILED` | O processamento falhou e a mensagem permanece elegível para uma nova tentativa. |

O instante `ProcessingStartedOnUtc` registra quando o processamento foi iniciado ou reiniciado. Essa informação permite determinar se uma mensagem em `PROCESSING` ainda representa um processamento ativo ou se ultrapassou o timeout configurado.

Quando o processamento termina com sucesso, `ProcessedOnUtc` registra sua conclusão. Em caso de falha, `Error` mantém a informação correspondente ao erro ocorrido.

Assim, a Inbox não mantém apenas um histórico de mensagens recebidas. Ela funciona também como um registro persistente do **estado do processamento** de cada evento.

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

Uma mensagem cujo processamento falhou não deve ser considerada concluída apenas porque foi recebida. Nesse caso, seu estado na Inbox é alterado para `FAILED`.

Quando o mecanismo de Retry entrega novamente a mesma mensagem, a Inbox permite que ela transite de `FAILED` para `PROCESSING`, registrando um novo início de processamento.

```text
PROCESSING
    ↓
  Falha
    ↓
 FAILED
    ↓
  Retry
    ↓
PROCESSING
    ↓
 Sucesso
    ↓
PROCESSED
```

Dessa forma, a Inbox impede a repetição de mensagens já concluídas sem bloquear novas tentativas de mensagens cujo processamento falhou.

---

# Timeout de Processamento

O estado `PROCESSING` indica que uma mensagem foi reivindicada por um Consumer e que seu processamento está em andamento.

Entretanto, um Worker pode ser interrompido depois de realizar o claim e antes de concluir o processamento. Nesse cenário, o registro poderia permanecer indefinidamente em `PROCESSING`.

Para evitar esse bloqueio permanente, o OrderFlow utiliza um **timeout de processamento**.

```text
PROCESSING
    ↓
Nova entrega
    ↓
Tempo limite expirou?
    │
    ├── Não
    │     ↓
    │  Mantém processamento atual
    │
    └── Sim
          ↓
      Considera processamento abandonado
          ↓
      Reinicia PROCESSING
          ↓
      Atualiza ProcessingStartedOnUtc
          ↓
      Executa novamente o handler
```

Quando uma nova entrega encontra uma mensagem em `PROCESSING`, o tempo decorrido desde `ProcessingStartedOnUtc` é comparado com o timeout configurado.

Se o timeout ainda não tiver expirado, a mensagem é considerada como estando legitimamente em processamento e outra instância do Consumer não deverá executar simultaneamente o handler.

Se o timeout tiver expirado, o processamento anterior é considerado abandonado. A mensagem poderá então ter seu processamento reiniciado, atualizando `ProcessingStartedOnUtc` antes de uma nova execução do handler.

No OrderFlow, esse tempo limite é configurável por meio das opções da Inbox, evitando que a regra de recuperação fique fixa na implementação.

> O timeout não representa uma falha da mensagem. Ele representa o limite utilizado para distinguir um processamento ainda ativo de um processamento possivelmente abandonado.

---

# Concorrência entre Consumers

Em um ambiente com múltiplas instâncias do Worker, duas entregas da mesma mensagem podem ser recebidas por Consumers diferentes.

O estado `PROCESSING` persistido antes da execução do handler permite que uma segunda instância identifique que aquela mensagem já possui um processamento em andamento.

```text
        Mesma mensagem
             │
       EventId = ABC
             │
       ┌─────┴─────┐
       ↓           ↓
   Consumer A   Consumer B
       │           │
       ↓           ↓
   Consulta      Consulta
    Inbox         Inbox
       │           │
       ↓           ↓
   Realiza       Encontra
    claim       PROCESSING
       │           │
       ↓           ↓
   PROCESSING   Não executa
       │        o handler
       ↓
   Executa
   handler
       │
       ↓
   PROCESSED
```

Quando uma instância obtém o claim, o estado `PROCESSING` é persistido e confirmado antes da execução do handler.

Enquanto esse processamento estiver dentro do timeout configurado, outra instância que receber o mesmo `EventId` reconhecerá que existe um processamento em andamento e não executará simultaneamente a lógica de negócio.

Após a conclusão bem-sucedida, o estado passa para `PROCESSED`. Uma entrega posterior do mesmo evento será então reconhecida como já processada e não executará novamente o handler.

Esse mecanismo reduz o risco de execução concorrente dos mesmos efeitos de negócio em um cenário com múltiplos Workers.

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
OrderFlow.Api
     ↓
Command
     ↓
Aggregate
     ↓
Domain Event
     ↓
Outbox
     ↓
Outbox Worker
     ↓
RabbitMQ
     ↓
Payments Worker
     ↓
Consumer
     ↓
Consulta Inbox pelo EventId
     ↓
Estado atual?
     │
     ├── PROCESSED
     │       ↓
     │      ACK
     │
     ├── PROCESSING
     │       ↓
     │   Verifica timeout
     │
     ├── FAILED
     │       ↓
     │   Reinicia PROCESSING
     │
     └── Não existe
             ↓
        Cria PROCESSING
             ↓
        Persiste o claim
             ↓
        Executa handler
             ↓
       ┌─────┴─────┐
       │           │
    Sucesso       Falha
       │           │
       ↓           ↓
   PROCESSED     FAILED
       │           │
       ↓           ↓
      ACK      Retry / DLQ
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

O Inbox Pattern garante o processamento idempotente de mensagens em arquiteturas distribuídas.

No OrderFlow, a Inbox:

* utiliza o `EventId` como identificador da mensagem;
* persiste o estado do processamento no SQL Server;
* realiza o claim da mensagem como `PROCESSING` antes da execução do handler;
* marca mensagens concluídas como `PROCESSED`;
* registra falhas utilizando o estado `FAILED`;
* permite que mensagens `FAILED` sejam processadas novamente pelo mecanismo de Retry;
* impede que mensagens `PROCESSED` executem novamente a lógica de negócio;
* identifica mensagens que já estão em `PROCESSING`;
* utiliza `ProcessingStartedOnUtc` e um timeout configurável para recuperar processamentos possivelmente abandonados;
* permite que múltiplas instâncias do Worker coordenem o processamento por meio do estado persistido da Inbox;
* trabalha em conjunto com Retry e Dead Letter Queue.

Com isso, o OrderFlow combina a garantia **At Least Once** do RabbitMQ com controle persistente de idempotência no lado consumidor.

Enquanto o **Outbox Pattern** garante a publicação confiável dos eventos pelo produtor, o **Inbox Pattern** controla o processamento confiável desses eventos pelo consumidor.