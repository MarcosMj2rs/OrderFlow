# ADR-017 — Saga Pattern

**Status:** Aceita
**Data:** 2026-10-12
**Projeto:** OrderFlow

## 1. Contexto

O OrderFlow utiliza uma arquitetura distribuída orientada a eventos, construída com C#/.NET 10, Clean Architecture, DDD, CQRS, MediatR, SQL Server e RabbitMQ.

O processamento de pedidos envolve múltiplas operações de negócio, incluindo:

- Criação do pedido.
- Processamento do pagamento.
- Reserva de inventário.
- Confirmação do pedido.

Essas operações não são executadas dentro de uma única transação de banco de dados.

Consequentemente, falhas durante o processamento podem deixar operações parcialmente concluídas.

Para coordenar essas operações, o OrderFlow adotou o **Saga Pattern com orquestração centralizada**, mantendo o estado da execução persistido na entidade `OrderSaga`.

O orquestrador é responsável por acompanhar a evolução do processo, identificar etapas concluídas e permitir a recuperação após falhas, evitando a repetição de efeitos de negócio já confirmados.

A implementação utiliza os mecanismos existentes de Outbox, Inbox, Retry e DLQ para apoiar a comunicação confiável e o reprocessamento de mensagens.

Nesta etapa, foram implementados e validados os mecanismos de execução, persistência de estado, idempotência sequencial e recuperação da Saga.

As operações compensatórias, como estorno de pagamento e liberação de inventário, serão implementadas em uma etapa posterior.

## 2. Decisão

O OrderFlow adotará o **Saga Pattern com orquestração centralizada** para coordenar o processamento distribuído de pedidos.

A responsabilidade pela coordenação das etapas pertence ao orquestrador da Saga, que determina quais operações devem ser executadas de acordo com o estado persistido.

A entidade `OrderSaga` representa o estado da execução e permite identificar as etapas já concluídas.

O fluxo principal contempla:

1. Criação do pedido.
2. Processamento do pagamento.
3. Reserva de inventário.
4. Confirmação do pedido como `PAID`.
5. Conclusão da Saga como `COMPLETED`.

A implementação deverá preservar as seguintes propriedades:

- **Persistência de estado:** o progresso da Saga deve sobreviver a reinicializações do processo.
- **Idempotência:** o reprocessamento não deve repetir efeitos de negócio já confirmados.
- **Recuperabilidade:** falhas técnicas devem permitir a retomada a partir do último estado persistido.
- **Consistência eventual:** as operações não dependem de uma transação distribuída global.
- **Observabilidade:** os estados persistidos devem permitir acompanhar e diagnosticar a execução.

A implementação atual contempla o fluxo principal e a recuperação de falhas técnicas.

As compensações de pagamento e inventário serão adicionadas posteriormente, preservando o modelo de orquestração adotado.

## 3. Alternativas consideradas

Foram avaliadas três abordagens para coordenar as operações distribuídas do processamento de pedidos.

### 3.1 Transação distribuída (Two-Phase Commit — 2PC)

Essa abordagem utiliza um coordenador de transações para garantir que todas as operações sejam confirmadas ou revertidas conjuntamente.

**Vantagens:**
- Consistência transacional forte.
- Modelo de confirmação ou reversão centralizado.

**Desvantagens:**
- Acoplamento entre os participantes.
- Dependência de suporte a transações distribuídas.
- Maior complexidade operacional.
- Possibilidade de bloqueios e redução da disponibilidade.

**Decisão:** Rejeitada.

O OrderFlow prioriza autonomia dos componentes, mensageria assíncrona e consistência eventual.

### 3.2 Saga Coreografada

Na coreografia, cada participante reage a eventos e publica novos eventos, sem um orquestrador central responsável pelo fluxo completo.

**Vantagens:**
- Menor acoplamento direto entre participantes.
- Distribuição das responsabilidades.
- Facilidade de evolução de fluxos simples.

**Desvantagens:**
- Dificuldade de visualizar o processo completo.
- Regras de coordenação distribuídas entre participantes.
- Maior complexidade de diagnóstico e recuperação.
- Fluxos compensatórios potencialmente difíceis de acompanhar.

**Decisão:** Não adotada para o processamento principal de pedidos.

### 3.3 Saga Orquestrada

A orquestração utiliza um componente central responsável por coordenar as etapas do processo.

O orquestrador consulta o estado persistido da Saga e determina quais operações devem ser executadas.

**Vantagens:**
- Fluxo de negócio explícito.
- Controle centralizado das transições.
- Persistência do progresso.
- Recuperação baseada no último estado confirmado.
- Melhor rastreabilidade.
- Facilidade para incorporar compensações.

**Desvantagens:**
- Maior responsabilidade do orquestrador.
- Necessidade de persistir e controlar estados.
- Risco de concentrar excessivamente regras de negócio.
- Necessidade de garantir idempotência e controle de concorrência.

**Decisão:** Aceita.

A Saga Orquestrada oferece o modelo mais adequado aos objetivos do laboratório OrderFlow, especialmente para estudar recuperação de falhas, idempotência, consistência eventual e compensações distribuídas.

## 4. Modelo de estados da Saga

O OrderFlow utiliza a entidade `OrderSaga` para persistir o progresso da execução do processo distribuído de pedidos.

Cada transição representa um marco do processamento e permite que o orquestrador identifique quais operações já foram concluídas.

### 4.1 Fluxo principal

O fluxo principal da Saga contempla as seguintes etapas:

1. Criação do pedido.
2. Processamento e confirmação do pagamento.
3. Reserva do inventário.
4. Atualização do pedido para `PAID`.
5. Conclusão da Saga como `COMPLETED`.

Os estados `PAYMENT_COMPLETED` e `INVENTORY_RESERVED` representam pontos de recuperação persistidos.

O estado `COMPLETED` indica que o fluxo principal da Saga foi concluído.

### 4.2 Persistência e recuperação

O estado da Saga é persistido no SQL Server.

Após uma falha técnica, o orquestrador deve consultar o estado persistido antes de decidir quais operações executar.

Foram validados os seguintes cenários:

- Recuperação a partir de `PAYMENT_COMPLETED`, sem repetir o pagamento.
- Recuperação a partir de `INVENTORY_RESERVED`, sem repetir a reserva.
- Recuperação após persistir o pedido como `PAID`, antes de persistir a Saga como `COMPLETED`.

No terceiro cenário, foi confirmado que apenas um `OrderPaidDomainEvent` foi gerado.

### 4.3 Regras de transição

As transições devem respeitar as seguintes regras:

- O estado persistido determina o ponto de retomada.
- Operações já confirmadas não devem produzir novos efeitos de negócio durante o reprocessamento.
- A conclusão da Saga depende da confirmação das etapas necessárias.
- O reprocessamento deve preservar a consistência entre o estado do pedido e o estado da Saga.
- Uma falha técnica não implica automaticamente a execução de compensações.

### 4.4 Evolução do modelo

O modelo será ampliado para contemplar compensações, incluindo estorno de pagamento e liberação de inventário.

Os estados e as transições específicos dessas operações serão definidos quando a implementação das compensações for iniciada.

Não serão documentados como implementados estados compensatórios que ainda não existam no código.

## 5. Idempotência e recuperação

A Saga do OrderFlow utiliza persistência de estado e operações idempotentes para permitir a recuperação após falhas técnicas.

O objetivo é garantir que o reprocessamento de uma mensagem não produza novamente efeitos de negócio já confirmados.

### 5.1 Idempotência sequencial

Os handlers responsáveis pelo processamento de pagamento e pela reserva de inventário possuem mecanismos de idempotência sequencial.

Antes de executar uma operação, o processamento verifica se o efeito de negócio correspondente já foi realizado.

Quando a operação já está concluída, o handler deve reconhecer essa condição e permitir a continuidade do fluxo sem repetir seus efeitos.

Essa proteção é especialmente importante porque a infraestrutura de mensageria pode entregar uma mesma mensagem mais de uma vez.

A idempotência sequencial não garante, isoladamente, proteção contra execuções simultâneas concorrentes.

### 5.2 Recuperação após PAYMENT_COMPLETED

Quando o pagamento já foi confirmado e o estado `PAYMENT_COMPLETED` foi persistido, uma falha posterior não deve provocar um novo pagamento.

Durante a recuperação, o orquestrador consulta o estado da Saga e continua a partir da etapa de reserva de inventário.

Esse cenário foi implementado e validado.

### 5.3 Recuperação após INVENTORY_RESERVED

Quando a reserva de inventário já foi concluída e o estado `INVENTORY_RESERVED` foi persistido, o reprocessamento não deve executar novamente a reserva.

O orquestrador reconhece a etapa concluída e continua o fluxo de confirmação do pedido.

Esse cenário foi implementado e validado.

### 5.4 Recuperação após Order PAID

Existe uma janela de falha entre a persistência do pedido como `PAID` e a persistência da Saga como `COMPLETED`.

Nesse cenário, o pedido já foi confirmado, mas a Saga ainda não registrou sua conclusão.

O reprocessamento deve reconhecer que o pedido já está pago e concluir a Saga sem executar novamente a confirmação do pedido.

Foi validado que essa recuperação produziu apenas um `OrderPaidDomainEvent`.

### 5.5 Integração com Inbox

O Inbox participa do controle de processamento das mensagens recebidas.

Foi validado o reprocessamento de uma mensagem anteriormente marcada como `FAILED`, que posteriormente passou para `PROCESSED`.

A recuperação da mensagem deve preservar a consistência do processamento e respeitar o estado persistido da Saga.

O Inbox e a Saga possuem responsabilidades complementares:

- O Inbox controla o processamento da mensagem.
- A Saga controla o progresso do processo distribuído.
- Os handlers idempotentes evitam a repetição dos efeitos de negócio já confirmados.

### 5.6 Garantias e limitações

A implementação atual fornece recuperação baseada em estado persistido e idempotência sequencial nos cenários validados.

Essa estratégia não deve ser interpretada como garantia universal de execução exatamente uma vez (*exactly-once execution*).

A proteção contra concorrência simultânea, quando necessária, deverá considerar mecanismos adicionais, como controle otimista de concorrência, restrições de unicidade ou coordenação transacional.

A combinação de persistência de estado, Inbox e idempotência permite construir um fluxo recuperável mesmo quando mensagens são entregues mais de uma vez.

## 6. Integração com a infraestrutura de mensageria

A Saga do OrderFlow utiliza a infraestrutura de mensageria existente para coordenar o processamento distribuído de pedidos.

A orquestração é responsável pelo fluxo de negócio, enquanto os componentes de mensageria fornecem mecanismos de publicação, entrega, reprocessamento e tratamento de falhas.

### 6.1 RabbitMQ

O RabbitMQ é utilizado como broker de mensagens, permitindo a comunicação assíncrona entre os componentes do OrderFlow.

A topologia de mensageria é inicializada pelo `RabbitMqTopologyHostedService`.

Esse serviço é responsável por preparar os recursos de mensageria configurados antes do processamento normal das mensagens.

A responsabilidade do RabbitMQ é transportar mensagens, não controlar os estados de negócio da Saga.

### 6.2 Outbox Pattern

O Outbox Pattern é utilizado para apoiar a publicação confiável de eventos decorrentes de alterações persistidas no banco de dados.

Seu objetivo é reduzir o risco de inconsistência entre a persistência de dados e a publicação de mensagens.

A Saga não substitui o Outbox.

Enquanto a Saga coordena as etapas do processo distribuído, o Outbox atua na confiabilidade da publicação de eventos.

### 6.3 Inbox Pattern

O Inbox Pattern controla o processamento das mensagens recebidas.

Ele permite identificar mensagens previamente processadas e acompanhar tentativas de processamento que apresentaram falhas.

No contexto da Saga, foi validado o reprocessamento de uma mensagem com estado `FAILED`, que posteriormente passou para `PROCESSED`.

O Inbox não substitui a persistência da entidade `OrderSaga`.

As responsabilidades são distintas:

- **Inbox:** controlar o processamento da mensagem.
- **OrderSaga:** registrar o progresso do processo de negócio.
- **Handlers idempotentes:** impedir a repetição de efeitos de negócio já confirmados.

### 6.4 Retry

O mecanismo de Retry permite realizar novas tentativas de processamento diante de falhas transitórias.

O reprocessamento deve respeitar a idempotência dos handlers e os estados persistidos da Saga.

Uma nova tentativa de entrega não significa que todas as etapas do processo de negócio devam ser executadas novamente.

O orquestrador deve identificar o último estado confirmado e retomar somente as operações necessárias.

### 6.5 Dead Letter Queue

A Dead Letter Queue (DLQ) é utilizada para encaminhar mensagens que não puderam ser processadas conforme as políticas de falha e tentativas configuradas.

Uma mensagem encaminhada à DLQ não significa necessariamente que a Saga deva executar compensações.

A decisão de compensar uma operação depende das regras de negócio e do estado persistido do processo.

### 6.6 Separação de responsabilidades

A arquitetura mantém responsabilidades específicas para cada mecanismo:

| Componente | Responsabilidade |
|---|---|
| RabbitMQ | Transporte de mensagens |
| Outbox | Publicação confiável de eventos |
| Inbox | Controle do processamento de mensagens |
| Retry | Novas tentativas após falhas transitórias |
| DLQ | Isolamento de mensagens não processáveis |
| OrderSaga | Persistência do progresso da Saga |
| Orquestrador | Coordenação das etapas de negócio |
| Handlers | Execução idempotente das operações |

Essa separação evita concentrar regras de mensageria e regras de negócio em um único componente.

### 6.7 Decisão arquitetural

**Decisão:** reutilizar a infraestrutura de mensageria confiável do OrderFlow, mantendo a Saga como responsável exclusivamente pela coordenação do processo distribuído.

A Saga não implementará mecanismos próprios de transporte, Retry ou DLQ quando essas responsabilidades já forem atendidas pela infraestrutura existente.

## 7. Consequências arquiteturais

A adoção do Saga Pattern com orquestração centralizada introduz benefícios e responsabilidades adicionais na arquitetura do OrderFlow.

### 7.1 Consequências positivas

**Coordenação explícita do processo**

O fluxo de processamento de pedidos passa a ser coordenado por um orquestrador, responsável por determinar as próximas operações com base no estado persistido da Saga.

**Recuperação após falhas**

A persistência do progresso na entidade `OrderSaga` permite retomar o processamento após interrupções, sem necessariamente reiniciar todas as operações.

**Idempotência das operações**

Os handlers de pagamento e reserva de inventário reconhecem operações previamente concluídas, evitando a repetição sequencial de efeitos de negócio.

**Consistência eventual**

O processamento distribuído não depende de uma transação global entre todos os participantes.

**Rastreabilidade**

Os estados persistidos permitem identificar o progresso da Saga e auxiliar no diagnóstico de falhas.

**Integração com a infraestrutura existente**

A solução reutiliza os mecanismos de Outbox, Inbox, Retry e DLQ, evitando a duplicação de responsabilidades de mensageria.

### 7.2 Consequências negativas

**Maior complexidade de implementação**

A arquitetura exige um orquestrador, persistência de estados e tratamento explícito das transições.

**Consistência não imediata**

Durante o processamento, os participantes podem apresentar estados temporariamente diferentes.

**Necessidade de idempotência**

A entrega de mensagens pode ocorrer mais de uma vez, exigindo proteção contra efeitos duplicados.

**Gerenciamento de estados intermediários**

Falhas podem ocorrer entre a conclusão de uma operação de negócio e a atualização do estado da Saga.

**Maior complexidade operacional**

O diagnóstico de falhas exige considerar o estado da Saga, os registros do Inbox e as mensagens presentes na infraestrutura de mensageria.

### 7.3 Limitações da implementação atual

A implementação validada contempla:

- Orquestração do processamento de pedidos.
- Persistência dos estados da Saga.
- Idempotência sequencial nos handlers de pagamento e reserva.
- Recuperação após falhas em pontos intermediários.
- Recuperação após persistência de `Order PAID`.
- Reprocessamento de mensagens pelo Inbox.

Ainda não foram implementadas nesta etapa:

- Compensações de pagamento e inventário.
- Garantias adicionais de idempotência sob concorrência simultânea.
- Um mecanismo completo de recuperação automática de Sagas interrompidas, independente dos reprocessamentos já validados.

Essas funcionalidades poderão ser incorporadas em evoluções posteriores.

### 7.4 Impacto na evolução do OrderFlow

A arquitetura adotada permite ampliar o processamento distribuído com novas operações e fluxos compensatórios.

As próximas evoluções previstas incluem:

1. Implementação do estorno de pagamento.
2. Implementação da liberação de inventário.
3. Definição dos estados de compensação.
4. Coordenação das compensações pelo orquestrador.
5. Testes de falhas durante as próprias compensações.

Essas evoluções deverão preservar a idempotência, a persistência de estado e a rastreabilidade estabelecidas na implementação atual.

## 8. Política de falhas

A Saga do OrderFlow adota uma política de tratamento de falhas baseada em persistência de estado, idempotência e recuperação controlada.

O objetivo é permitir a continuidade do processamento distribuído sem repetir efeitos de negócio já confirmados.

### 8.1 Classificação das falhas

As falhas são classificadas em duas categorias principais.

**Falhas técnicas ou transitórias**

São falhas que não representam necessariamente uma impossibilidade de concluir o processo de negócio.

Exemplos:

- Indisponibilidade temporária do SQL Server.
- Interrupção do Worker durante o processamento.
- Falhas temporárias de comunicação.
- Exceções durante a execução de uma etapa.
- Falhas entre a persistência de uma operação e a atualização do estado da Saga.

Essas falhas devem ser tratadas prioritariamente por recuperação e reprocessamento idempotente.

**Falhas definitivas de negócio**

São situações nas quais uma operação não pode ser concluída por uma restrição ou decisão de negócio.

Exemplos:

- Pagamento recusado definitivamente.
- Estoque insuficiente para reservar os produtos.
- Pedido que não pode mais ser confirmado.

Essas situações poderão exigir compensações das operações anteriormente concluídas.

### 8.2 Política de recuperação

O orquestrador deve consultar o estado persistido da entidade `OrderSaga` antes de executar novas operações.

As etapas já confirmadas não devem produzir novamente seus efeitos de negócio.

Foram implementados e validados os seguintes cenários:

1. Recuperação após persistir `PAYMENT_COMPLETED`.
2. Recuperação após persistir `INVENTORY_RESERVED`.
3. Recuperação após persistir `Order PAID`, antes de `Saga COMPLETED`.

A recuperação deve respeitar os estados persistidos tanto da Saga quanto das entidades de negócio envolvidas.

### 8.3 Retry e Inbox

As falhas transitórias podem ser tratadas pelos mecanismos de Retry e reprocessamento do Inbox.

O Inbox permite controlar o processamento das mensagens recebidas.

Foi validado o reprocessamento de uma mensagem anteriormente marcada como `FAILED`, com posterior transição para `PROCESSED`.

O reprocessamento não deve reiniciar indiscriminadamente todas as operações da Saga.

### 8.4 Dead Letter Queue

Mensagens que não puderem ser processadas conforme a política de tentativas configurada poderão ser encaminhadas à DLQ.

O encaminhamento de uma mensagem à DLQ não representa automaticamente uma falha definitiva de negócio.

A decisão sobre compensar operações deve considerar o estado persistido da Saga e as regras do domínio.

### 8.5 Falhas em pontos intermediários

Uma falha pode ocorrer após a confirmação de uma operação de negócio, mas antes da persistência do próximo estado da Saga.

Por esse motivo, a recuperação não pode depender exclusivamente da última transição registrada em `OrderSaga`.

Os handlers devem reconhecer operações previamente concluídas.

Essa estratégia foi validada no cenário em que o pedido já estava `PAID`, mas a Saga ainda não estava `COMPLETED`.

Nesse teste, foi produzido apenas um `OrderPaidDomainEvent`.

### 8.6 Política de compensações

As compensações serão introduzidas na próxima etapa de desenvolvimento.

Estão previstas:

- Compensação de pagamento por meio de estorno.
- Compensação de reserva por meio da liberação de inventário.

As operações compensatórias deverão ser idempotentes e rastreáveis.

A compensação não deve ser iniciada automaticamente em resposta a qualquer exceção técnica.

Sua execução dependerá de uma decisão explícita do fluxo de negócio e do estado da Saga.

Falhas durante a compensação também deverão permitir recuperação segura.

### 8.7 Garantias e limitações

A implementação atual fornece recuperação baseada em estados persistidos e idempotência sequencial nos cenários testados.

Não existe garantia universal de execução exatamente uma vez (*exactly-once execution*) em todos os componentes distribuídos.

Também não se considera concluída a implementação das compensações ou de mecanismos adicionais de controle de concorrência.

### 8.8 Decisão arquitetural

**Decisão:** separar recuperação técnica de compensação de negócio.

Falhas técnicas recuperáveis deverão utilizar reprocessamento idempotente e retomada a partir do estado persistido.

Falhas definitivas de negócio poderão iniciar compensações, conforme as regras que serão implementadas na próxima evolução da Saga.

Essa abordagem preserva a consistência eventual e evita a utilização de transações distribuídas globais.


