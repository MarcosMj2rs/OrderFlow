# Retry

**Status:** Aceita

---

# Objetivo

Retry é o mecanismo utilizado para realizar uma nova tentativa de processamento quando uma mensagem apresenta uma **falha transitória**.

No **OrderFlow**, o Retry é utilizado no consumo de mensagens RabbitMQ para evitar que falhas temporárias provoquem perda imediata da mensagem ou ciclos de reprocessamento sem controle.

Exemplos de falhas transitórias:

- indisponibilidade temporária de banco de dados;
- timeout em serviço externo;
- falha de rede;
- dependência momentaneamente indisponível;
- lock ou concorrência transitória.

---

# Problema

Uma estratégia simples de recuperação poderia utilizar:

```text
NACK + requeue = true
```

Nesse modelo, a mensagem retorna imediatamente para a fila principal:

```text
Consumer
    ↓
Erro
    ↓
NACK + requeue
    ↓
Fila principal
    ↓
Consumer
```

Se a causa da falha continuar existindo, esse comportamento pode gerar um ciclo contínuo de processamento.

Além disso, não existe um intervalo controlado entre as tentativas.

Por esse motivo, o **OrderFlow** utiliza uma fila intermediária de Retry.

---

# Retry Queue

Quando ocorre uma falha transitória, a mensagem é republicada em uma fila específica:

```text
orderflow.order-created.retry
```

O fluxo básico é:

```text
Main Queue
    ↓
Consumer
    ↓
Falha transitória
    ↓
Retry Queue
    ↓
Espera
    ↓
Main Queue
    ↓
Nova tentativa
```

Durante o período de espera, a mensagem permanece armazenada no RabbitMQ.

O Consumer não precisa permanecer bloqueado aguardando a próxima tentativa.

---

# Backoff Exponencial

O **OrderFlow** utiliza **backoff exponencial** para aumentar progressivamente o intervalo entre as tentativas.

A configuração inicial utiliza:

```text
RetryBaseDelayMilliseconds = 10000
```

Com três tentativas, os intervalos são:

```text
Retry 1 → 10 segundos
Retry 2 → 20 segundos
Retry 3 → 40 segundos
```

O cálculo utilizado é:

```text
delay = baseDelay × 2^(retryCount - 1)
```

Isso evita realizar novas tentativas em intervalos muito curtos quando uma dependência continua indisponível.

---

# Expiration

O atraso de cada tentativa é definido individualmente na mensagem utilizando a propriedade AMQP:

```text
Expiration
```

Exemplo:

```text
Retry 1
Expiration = 10000

Retry 2
Expiration = 20000

Retry 3
Expiration = 40000
```

A Retry Queue não utiliza um `x-message-ttl` fixo para controlar esses intervalos.

Cada mensagem recebe seu próprio tempo de expiração.

---

# Dead Lettering da Retry Queue

A Retry Queue possui:

```text
x-dead-letter-exchange
x-dead-letter-routing-key
```

Quando a mensagem expira, o RabbitMQ realiza o dead lettering para a exchange principal.

O fluxo é:

```text
Retry Queue
    ↓
Expiration
    ↓
Dead Letter Exchange
    ↓
Main Exchange
    ↓
Main Queue
```

A mensagem fica novamente disponível para processamento.

---

# Controle das Tentativas

O número de tentativas é armazenado no header:

```text
x-orderflow-retry-count
```

Exemplo:

```text
x-orderflow-retry-count = 1
```

A cada falha transitória:

```text
Lê RetryCount
    ↓
Incrementa RetryCount
    ↓
Calcula Backoff
    ↓
Define Expiration
    ↓
Publica na Retry Queue
```

O limite inicial é:

```text
3 tentativas
```

---

# Fluxo Completo de Retry

```text
Mensagem
    ↓
Main Queue
    ↓
Consumer
    ↓
Falha transitória
    ↓
Retry 1 - 10s
    ↓
Main Queue
    ↓
Falha transitória
    ↓
Retry 2 - 20s
    ↓
Main Queue
    ↓
Falha transitória
    ↓
Retry 3 - 40s
    ↓
Main Queue
```

Se o processamento continuar falhando após o limite configurado:

```text
DLQ
```

---

# Falhas Permanentes

Nem toda falha deve passar pelo Retry.

Falhas permanentes são aquelas em que repetir o processamento não resolverá o problema.

Exemplos:

- JSON inválido;
- identificadores obrigatórios inválidos;
- propriedades obrigatórias ausentes;
- contrato incompatível;
- dados semanticamente inválidos.

Nesse caso:

```text
Consumer
    ↓
Falha permanente
    ↓
DLQ
```

A mensagem não passa pela Retry Queue.

---

# Confirmação da Publicação

Ao republicar uma mensagem para Retry, o **OrderFlow** não confirma imediatamente a mensagem original.

O fluxo é:

```text
Mensagem original
    ↓
Publica na Retry Queue
    ↓
Confirma a publicação
    ↓
ACK da mensagem original
```

O mesmo princípio é utilizado quando uma mensagem é publicada diretamente na DLQ.

Isso reduz o risco de perda da mensagem caso a republicação falhe.

---

# At Least Once

O mecanismo de Retry mantém o modelo de entrega:

```text
At Least Once
```

Isso significa que uma mesma mensagem pode ser entregue mais de uma vez.

Por esse motivo, o processamento precisa ser idempotente.

No **OrderFlow**, essa proteção é complementada por:

```text
Inbox Pattern
+
Idempotência de negócio
```

---

# Relação com Inbox

Retry e Inbox resolvem problemas diferentes.

```text
Retry
    ↓
Controla novas tentativas após falhas transitórias
```

```text
Inbox
    ↓
Controla processamento duplicado de mensagens
```

Eles trabalham em conjunto:

```text
RabbitMQ
    ↓
Retry
    ↓
Consumer
    ↓
Inbox
    ↓
Processamento
```

---

# Relação com DLQ

Retry tenta recuperar mensagens que apresentaram falhas transitórias.

A DLQ recebe mensagens que:

```text
Excederam o limite de Retry
```

ou:

```text
Apresentaram falha permanente
```

Portanto:

```text
Retry
    ↓
Tenta recuperar

DLQ
    ↓
Isola mensagens não processáveis
```

A estratégia detalhada da DLQ é documentada separadamente.

---

# Limitação da Estratégia Atual

A implementação atual utiliza uma única fila clássica de Retry e define diferentes valores de `Expiration` por mensagem.

Essa estratégia funciona para o fluxo implementado, mas não deve ser interpretada como garantia de temporização independente perfeita quando várias mensagens com diferentes tempos de expiração estiverem intercaladas na mesma fila.

Uma evolução futura poderá utilizar:

```text
Retry Queue 10s
Retry Queue 20s
Retry Queue 40s
```

ou outro mecanismo específico de agendamento de mensagens.

---

# Exemplo no OrderFlow

Para `OrderCreatedMessage`:

```text
orderflow.order-created
    ↓
OrderCreatedConsumer
    ↓
Falha transitória
    ↓
Calcula RetryCount
    ↓
Calcula Backoff
    ↓
orderflow.order-created.retry
    ↓
Expiration
    ↓
orderflow.order-created
```

Se o limite for excedido:

```text
orderflow.order-created.dlq
```

---

# Resumo

O Retry do **OrderFlow** utiliza:

```text
RabbitMQ
+
Retry Queue
+
Backoff exponencial
+
Expiration por mensagem
+
RetryCount
+
Publisher Confirmation
+
Manual ACK
+
DLQ
```

Com os intervalos iniciais:

```text
10s → 20s → 40s
```

O objetivo é permitir a recuperação controlada de falhas transitórias sem bloquear o Consumer, sem utilizar requeue imediato como estratégia permanente e sem permitir tentativas infinitas.

---

# Documentos Relacionados

- ADR-007 — RabbitMQ
- ADR-010 — Inbox Pattern
- ADR-011 — Idempotência
- ADR-012 — Retry
- ADR-013 — Dead Letter Queue