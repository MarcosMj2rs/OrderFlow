# DEC-012 - Por que Retry

**Status:** Aceita

---

# Contexto

O **OrderFlow** utiliza RabbitMQ para processamento assíncrono de mensagens.

Durante o consumo, algumas falhas podem ser temporárias, como:

- indisponibilidade momentânea de uma dependência;
- timeout;
- falha de rede;
- lock ou concorrência transitória;
- indisponibilidade temporária de banco de dados.

Nesses casos, descartar imediatamente a mensagem pode causar perda de processamento.

Por outro lado, devolver a mensagem imediatamente para a fila principal utilizando:

```text
NACK + requeue = true
```

pode criar ciclos contínuos de reprocessamento enquanto a causa da falha permanecer ativa.

Era necessário, portanto, adotar uma estratégia que permitisse novas tentativas de forma controlada.

---

# Decisão

O **OrderFlow** utilizará uma **Retry Queue** para tratar falhas transitórias durante o consumo de mensagens RabbitMQ.

As novas tentativas utilizarão **backoff exponencial**, com intervalo definido individualmente por mensagem através da propriedade AMQP:

```text
Expiration
```

A configuração inicial será:

```text
Retry 1 → 10 segundos
Retry 2 → 20 segundos
Retry 3 → 40 segundos
```

O número da tentativa será transportado no header:

```text
x-orderflow-retry-count
```

Após o limite configurado, a mensagem será encaminhada para a **Dead Letter Queue**.

Falhas classificadas como permanentes não passarão pelo Retry e serão encaminhadas diretamente para a DLQ.

---

# Por que não utilizar requeue imediato?

O mecanismo:

```text
NACK + requeue = true
```

devolve imediatamente a mensagem para a fila principal.

Se a causa da falha continuar existindo:

```text
Consumer
    ↓
Falha
    ↓
Requeue
    ↓
Consumer
    ↓
Falha
    ↓
Requeue
```

o sistema pode entrar em um ciclo de reprocessamento sem intervalo e sem limite explícito.

Isso pode provocar:

- consumo excessivo de recursos;
- grande volume de logs;
- processamento repetitivo;
- dificuldade de diagnóstico;
- impacto sobre outras mensagens.

Por esse motivo, `requeue: true` não será utilizado como estratégia permanente de Retry.

---

# Por que não utilizar `Task.Delay`?

Outra possibilidade seria manter a mensagem no Consumer e aguardar antes da próxima tentativa:

```text
Consumer
    ↓
Falha
    ↓
Task.Delay
    ↓
Nova tentativa
```

Essa abordagem manteria a entrega pendente durante o período de espera.

Além disso:

- o Consumer permaneceria ocupado;
- a mensagem permaneceria `Unacked`;
- um reinício do Worker interromperia o processo local;
- o RabbitMQ deixaria de controlar o período de espera.

A Retry Queue permite que a mensagem permaneça armazenada no broker enquanto o Consumer é liberado.

---

# Por que utilizar backoff exponencial?

Realizar todas as tentativas com o mesmo intervalo pode pressionar repetidamente uma dependência que continua indisponível.

Com backoff exponencial, o intervalo aumenta progressivamente:

```text
10s → 20s → 40s
```

Isso reduz a frequência das novas tentativas quando uma falha persiste.

O cálculo utilizado é:

```text
delay = baseDelay × 2^(retryCount - 1)
```

---

# Por que utilizar `Expiration` por mensagem?

A implementação inicial utilizava um intervalo fixo na Retry Queue.

Com a evolução para backoff exponencial, cada tentativa precisa possuir um intervalo diferente.

Por isso, o atraso passou a ser definido individualmente através da propriedade AMQP:

```text
Expiration
```

Exemplo:

```text
Retry 1 → Expiration = 10000
Retry 2 → Expiration = 20000
Retry 3 → Expiration = 40000
```

A Retry Queue permanece responsável por armazenar temporariamente a mensagem.

Quando a mensagem expira, o RabbitMQ utiliza o dead lettering configurado para devolvê-la ao fluxo principal.

---

# Por que confirmar a republicação antes do ACK?

Quando ocorre uma falha transitória, a mensagem precisa sair da fila principal e entrar na Retry Queue.

O fluxo utilizado é:

```text
Mensagem original
    ↓
Publica na Retry Queue
    ↓
Confirma a publicação
    ↓
ACK da mensagem original
```

Se o `ACK` fosse realizado antes da confirmação da republicação, uma falha durante a publicação poderia provocar perda da mensagem.

Por isso, a entrega original somente é confirmada após a confirmação da nova publicação.

O mesmo princípio é utilizado para publicação na DLQ.

---

# Por que limitar o número de tentativas?

Uma falha transitória pode deixar de ser transitória na prática.

Uma dependência pode permanecer indisponível por um período prolongado ou determinada mensagem pode continuar provocando falhas.

Por isso, o Retry precisa possuir um limite.

No **OrderFlow**, a configuração inicial utiliza:

```text
3 tentativas
```

Depois disso:

```text
Mensagem
    ↓
Retry 1
    ↓
Retry 2
    ↓
Retry 3
    ↓
DLQ
```

Isso impede reprocessamento indefinido.

---

# Falhas permanentes

Retry somente é útil quando existe possibilidade de sucesso em uma nova tentativa.

Falhas permanentes, como mensagens inválidas, não devem consumir tentativas desnecessariamente.

O fluxo é:

```text
Falha permanente
    ↓
Publica diretamente na DLQ
    ↓
Confirma a publicação
    ↓
ACK da mensagem original
```

Portanto, falhas permanentes não passam pela Retry Queue.

---

# Relação com Idempotência

O Retry mantém o modelo de entrega:

```text
At Least Once
```

Consequentemente, uma mensagem pode ser entregue mais de uma vez.

Retry não elimina duplicidade.

Por isso, o mecanismo deve trabalhar em conjunto com:

- Inbox Pattern;
- idempotência de mensagem;
- idempotência de negócio.

A estratégia de idempotência está documentada na `DEC-009`.

---

# Limitação conhecida

A implementação atual utiliza uma única fila clássica de Retry com diferentes valores de `Expiration` por mensagem.

Essa abordagem é suficiente para o estágio atual do **OrderFlow**, mas não representa uma garantia de temporização independente perfeita quando várias mensagens com diferentes tempos de expiração estiverem intercaladas na mesma fila.

Como evolução futura poderão ser avaliadas alternativas como:

```text
Retry Queue 10s
Retry Queue 20s
Retry Queue 40s
```

ou outro mecanismo específico de agendamento.

---

# Consequências

## Positivas

- evita ciclos infinitos de `requeue`;
- permite intervalo controlado entre tentativas;
- libera o Consumer durante a espera;
- mantém a mensagem armazenada no RabbitMQ;
- permite limitar o número de tentativas;
- reduz pressão sobre dependências temporariamente indisponíveis;
- integra o fluxo de Retry com a DLQ;
- preserva o modelo At Least Once.

## Negativas

- aumenta a complexidade da topologia RabbitMQ;
- exige republicação das mensagens;
- exige controle de headers;
- exige confirmação segura da republicação antes do ACK;
- exige idempotência no processamento;
- pode alterar a ordem de processamento das mensagens;
- uma única Retry Queue com diferentes valores de `Expiration` possui limitações de temporização quando várias mensagens são intercaladas.

---

# Resultado

A Retry Queue fornece ao **OrderFlow** um mecanismo controlado de recuperação de falhas transitórias.

A estratégia adotada combina:

```text
Retry Queue
+
Backoff exponencial
+
Expiration por mensagem
+
Retry Count
+
Publisher Confirmation
+
Manual ACK
+
DLQ
```

permitindo novas tentativas sem manter o Consumer bloqueado e sem utilizar requeue imediato como estratégia permanente.

---

# Documentos Relacionados

- ADR-007 — RabbitMQ
- ADR-010 — Inbox Pattern
- ADR-011 — Idempotência
- ADR-012 — Retry
- ADR-013 — Dead Letter Queue
- DEC-009 — Por que Idempotência
- Concept 11 — Retry