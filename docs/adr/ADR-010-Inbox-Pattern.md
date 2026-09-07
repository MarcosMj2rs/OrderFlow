# ADR-010 - Inbox Pattern

**Status:** Aceita

## Registro de Decisões

| ID  | Decisão                                                                                                                          											   |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| D01 | O OrderFlow utilizará o Inbox Pattern para garantir processamento idempotente das mensagens recebidas.                           											   |
| D02 | Cada evento recebido será identificado pelo `EventId` original gerado pelo produtor.                                             											   |
| D03 | O `EventId` será utilizado para detectar mensagens já processadas.                                                               											   |
| D04 | O registro da Inbox será persistido no SQL Server.                                                                               											   |
| D05 | O processamento da mensagem e a atualização de seu estado na Inbox deverão ocorrer de forma consistente.                         											   |
| D06 | Mensagens já processadas não executarão novamente a lógica de negócio.                                                           											   |
| D07 | Mensagens duplicadas deverão ser reconhecidas e confirmadas com ACK, evitando redelivery desnecessário.                          											   |
| D08 | A implementação será integrada inicialmente ao `OrderFlow.Worker.Payments`.                                                      											   |
| D09 | A infraestrutura da Inbox permanecerá na camada `Infrastructure`, preservando o Domain independente de mecanismos de mensageria. 											   |
| D10 | A estratégia deverá ser compatível com Retry, Dead Letter Queue e o modelo At Least Once Delivery já adotado pelo OrderFlow.     											   |
| D11 | Uma mensagem nova será registrada inicialmente com o estado `PROCESSING` antes da execução da lógica de negócio. 															   |
| D12 | O claim da mensagem será persistido e confirmado em transação própria antes da execução do handler, tornando o estado `PROCESSING` visível para outras instâncias do Consumer. |
| D13 | Mensagens em estado `PROCESSED` não executarão novamente a lógica de negócio e serão reconhecidas como já processadas. 														   |
| D14 | Mensagens em estado `FAILED` poderão retornar para `PROCESSING` em uma nova tentativa. 																						   |
| D15 | Mensagens que permanecerem em `PROCESSING` além do tempo limite configurado serão consideradas abandonadas e poderão ter seu processamento reiniciado. 						   |
| D16 | Mensagens em `PROCESSING` cujo timeout ainda não tenha expirado não serão processadas simultaneamente por outra instância do Consumer. 										   |
| D17 | O tempo limite de processamento da Inbox será configurável, permitindo adequação ao tempo esperado das operações executadas pelos Consumers. 								   |

## Escopo desta ADR

Esta ADR define a estratégia utilizada para garantir o processamento idempotente de eventos recebidos pelos consumidores do OrderFlow.

Estão contemplados:

* identificação das mensagens por `EventId`;
* persistência das mensagens recebidas;
* detecção de mensagens duplicadas;
* controle do estado de processamento;
* integração com os Consumers;
* confirmação de mensagens duplicadas por ACK;
* integração com o modelo At Least Once Delivery.

Não fazem parte desta ADR:

* regras específicas de pagamento;
* implementação de Saga;
* observabilidade distribuída;
* retenção e limpeza histórica da Inbox;
* versionamento de eventos.

> **Categoria:** Messaging / Reliability
>
> **Relacionadas:**
>
> * ADR-007 — RabbitMQ
> * ADR-009 — Outbox Pattern
> * ADR-012 — Retry
> * ADR-013 — Dead Letter Queue

---

# Contexto

O OrderFlow utiliza RabbitMQ com o modelo de entrega **At Least Once**, no qual uma mensagem pode ser entregue mais de uma vez ao consumidor.

Mesmo com o **Transactional Outbox Pattern** garantindo que os eventos sejam publicados de forma confiável, o consumidor ainda precisa lidar com possíveis redeliveries.

Considere o seguinte cenário:

```text
RabbitMQ
    ↓
Worker.Payments
    ↓
Processa mensagem
    ↓
Executa regra de negócio
    ↓
Falha antes do ACK
    ↓
RabbitMQ realiza redelivery
    ↓
Mensagem é processada novamente
```

Nesse cenário, embora o primeiro processamento tenha sido concluído, o RabbitMQ não recebeu a confirmação por meio do ACK e poderá entregar novamente a mesma mensagem.

O consumidor precisa, portanto, ser capaz de identificar que determinado evento já foi processado.

---

# Problema

O modelo **At Least Once Delivery** garante que uma mensagem não seja silenciosamente perdida, mas permite entregas duplicadas.

Sem um mecanismo de deduplicação, o mesmo evento poderá executar a lógica de negócio mais de uma vez.

Em operações sensíveis, isso pode provocar efeitos indesejados, como:

* processamento duplicado de pagamentos;
* atualização repetida do estado de uma entidade;
* execução duplicada de integrações externas;
* geração duplicada de novos eventos;
* inconsistência entre serviços.

O consumidor não pode depender exclusivamente do RabbitMQ para garantir processamento único.

É necessário persistir uma identificação das mensagens já processadas e verificar essa informação antes da execução da lógica de negócio.

---

# Drivers Arquiteturais

## Idempotência

O processamento repetido da mesma mensagem não deverá produzir efeitos de negócio duplicados.

---

## Confiabilidade

O consumidor deverá reconhecer mensagens já processadas mesmo após reinicializações do Worker ou novas entregas realizadas pelo RabbitMQ.

---

## Recuperação

O mecanismo deverá funcionar corretamente em cenários de falha e redelivery, permitindo que mensagens não concluídas sejam processadas novamente sem considerar como concluído um processamento que falhou.

---

## Consistência

O estado persistido na Inbox deverá representar corretamente o resultado do processamento da mensagem, evitando que uma mensagem seja marcada como processada antes da conclusão da operação correspondente.

---

## Baixo Acoplamento

A camada **Domain** permanecerá independente do mecanismo de Inbox e dos detalhes de mensageria.

A infraestrutura de deduplicação será mantida fora do domínio e integrada ao fluxo de consumo por meio de abstrações apropriadas.

---

# Alternativas Consideradas

## Confiar apenas no ACK e Redelivery do RabbitMQ

**Vantagens**

* implementação simples;
* nenhuma persistência adicional.

**Desvantagens**

* não impede o processamento duplicado;
* uma falha após a execução da regra de negócio e antes do ACK pode provocar novo processamento;
* não mantém histórico persistente das mensagens processadas.

**Decisão:** Rejeitada.

---

## Deduplicação em memória

Manter os `EventId` processados em memória no próprio Worker.

**Vantagens**

* implementação simples;
* consulta rápida;
* não exige acesso adicional ao banco de dados.

**Desvantagens**

* o histórico é perdido quando o processo é reiniciado;
* não funciona de forma confiável com múltiplas instâncias do Consumer;
* não oferece garantia durável de idempotência.

**Decisão:** Rejeitada.

---

## Inbox Pattern persistido no SQL Server

Funcionamento:

```text
RabbitMQ
    ↓
Consumer
    ↓
EventId
    ↓
Inbox
    ↓
Já processado?
    ├── Sim → ACK
    └── Não → Processamento
                    ↓
              Marca processado
                    ↓
                   ACK
```

**Vantagens**

* deduplicação persistente;
* mantém o controle após reinicializações do Worker;
* compatível com múltiplas instâncias de Consumers;
* complementa o modelo At Least Once Delivery;
* integra-se ao SQL Server já utilizado pela solução.

**Desvantagens**

* adiciona persistência ao fluxo de consumo;
* aumenta a complexidade do processamento;
* exige estratégia futura de retenção e limpeza dos registros.

**Decisão:** Aceita.

---

# Decisão

O OrderFlow utilizará o **Inbox Pattern** para controlar o processamento idempotente das mensagens recebidas.

Cada evento será identificado pelo `EventId` original gerado pelo produtor. Esse identificador será persistido no SQL Server e utilizado para detectar mensagens já processadas.

Antes de executar novamente a lógica de negócio, o fluxo de consumo verificará o estado correspondente na Inbox. Mensagens já concluídas não executarão novamente seus efeitos de negócio e poderão ser confirmadas com ACK.

Mensagens cujo processamento não tenha sido concluído deverão permanecer elegíveis para nova tentativa, preservando a compatibilidade com Retry, Dead Letter Queue e com o modelo **At Least Once Delivery** adotado pelo OrderFlow.

O processamento da Inbox será dividido em duas fases. Na primeira, o Consumer realizará o **claim** da mensagem, persistindo o estado `PROCESSING` em uma transação própria antes da execução da lógica de negócio. Essa transação será concluída antes da execução do handler, permitindo que outras instâncias do Consumer observem que a mensagem já está em processamento.

Após o claim, o handler executará a lógica correspondente à mensagem. Quando o processamento for concluído com sucesso, o registro será alterado para `PROCESSED`. Caso ocorra uma falha, o registro será alterado para `FAILED`, permanecendo elegível para uma nova tentativa.

Uma mensagem encontrada em `PROCESSING` não será processada simultaneamente por outra instância enquanto seu tempo limite de processamento não tiver expirado. Caso esse limite seja excedido, o processamento será considerado abandonado e uma nova tentativa poderá reiniciar o estado `PROCESSING`, atualizando o instante de início do processamento.

O tempo limite será configurável por meio das opções da Inbox, permitindo distinguir um processamento legítimo em andamento de um processamento abandonado em decorrência de falha ou interrupção do Worker.

A implementação será integrada inicialmente ao `OrderFlow.Worker.Payments`, enquanto os detalhes de persistência permanecerão na camada `Infrastructure`, mantendo o Domain independente do mecanismo de mensageria.

---

# Fluxo Arquitetural

```mermaid
flowchart TD

    A[RabbitMQ Queue]
    --> B[Consumer]
    --> C[Consulta Inbox pelo EventId]

    C --> D{Estado atual}

    D -->|PROCESSED| E[Não executa o handler]
    E --> F[ACK]

    D -->|PROCESSING| G{Timeout expirou?}
    G -->|Não| H[ALREADY_PROCESSING]
    G -->|Sim| I[Reinicia PROCESSING]

    D -->|FAILED| J[Retorna para PROCESSING]
    D -->|Mensagem nova| K[Cria registro PROCESSING]

    I --> L[Commit do claim]
    J --> L
    K --> L

    L --> M[Executa handler]

    M -->|Sucesso| N[Marca PROCESSED]
    N --> F

    M -->|Falha| O[Marca FAILED]
    O --> P[Retry / DLQ]
```

O fluxo separa explicitamente o **claim da mensagem** da execução do handler. O estado `PROCESSING` é persistido e confirmado antes da lógica de negócio, permitindo que outras instâncias do Consumer detectem um processamento já em andamento.

Mensagens `PROCESSED` não executam novamente o handler. Mensagens `FAILED` permanecem elegíveis para nova tentativa, enquanto mensagens em `PROCESSING` somente poderão ser retomadas quando o timeout configurado tiver expirado.

---

# Responsabilidades

## Consumer

Responsável por:

* receber e desserializar a mensagem proveniente do RabbitMQ;
* disponibilizar o `EventId` utilizado pelo mecanismo de Inbox;
* executar a lógica de processamento somente quando a mensagem ainda não tiver sido concluída;
* confirmar com ACK mensagens processadas com sucesso ou já reconhecidas como concluídas.

---

## Inbox

Responsável por:

* persistir a identificação das mensagens recebidas por meio do `EventId`;
* controlar os estados `PROCESSING`, `PROCESSED` e `FAILED`;
* realizar o claim persistente da mensagem antes da execução do handler;
* impedir o processamento concorrente de uma mensagem cujo estado `PROCESSING` ainda esteja dentro do timeout configurado;
* permitir a retomada de mensagens em estado `FAILED`;
* permitir a recuperação de mensagens que permaneceram em `PROCESSING` além do timeout configurado;
* registrar a conclusão do processamento por meio do estado `PROCESSED`;
* impedir a repetição dos efeitos de negócio de mensagens já concluídas.

---

## Infrastructure

Responsável por:

* implementar a persistência da Inbox no SQL Server;
* fornecer os componentes necessários para consulta e atualização dos registros da Inbox;
* integrar o mecanismo de Inbox ao fluxo de consumo sem introduzir dependências de infraestrutura no Domain.

---

## RabbitMQ

Responsável por:

* entregar as mensagens aos Consumers;
* realizar redelivery quando uma mensagem não for confirmada;
* manter o modelo **At Least Once Delivery** adotado pela solução.

O RabbitMQ não é responsável pela deduplicação dos efeitos de negócio. Essa responsabilidade pertence ao mecanismo de Inbox no lado consumidor.

---

# Consequências Positivas

* processamento idempotente das mensagens recebidas;
* proteção contra efeitos de negócio duplicados em cenários de redelivery;
* deduplicação persistente mesmo após reinicializações do Worker;
* compatibilidade com múltiplas instâncias de Consumers;
* complementa o modelo **At Least Once Delivery**;
* mantém o Domain independente dos mecanismos de mensageria e deduplicação.

---

# Consequências Negativas

* necessidade de persistência adicional no fluxo de consumo;
* aumento da complexidade do processamento das mensagens;
* consultas adicionais ao banco de dados;
* necessidade futura de estratégia de retenção e limpeza da Inbox.

---

# Trade-offs

| Decisão                                  | Benefício                              | Custo                                          |
| ---------------------------------------- | -------------------------------------- | ---------------------------------------------- |
| Inbox persistida                         | Deduplicação durável                   | Mais operações no banco                        |
| `EventId` como identificador da mensagem | Detecção consistente de duplicidades   | Dependência de identificador estável no evento |
| Verificação antes do processamento       | Evita repetição dos efeitos de negócio | Consulta adicional                             |
| ACK para mensagens já processadas        | Interrompe redelivery desnecessário    | Exige controle correto do estado da Inbox      |
| SQL Server                               | Reutiliza infraestrutura existente     | Crescimento da tabela da Inbox                 |

---

# Impacto nas Camadas

## Domain

Nenhum impacto direto. O Domain permanece independente do Inbox Pattern e dos detalhes de mensageria.

---

## Application

As abstrações necessárias ao processamento idempotente poderão ser expostas para permitir a integração sem dependência direta da infraestrutura.

---

## Infrastructure

Implementação da persistência da Inbox no SQL Server, incluindo modelo, configuração do Entity Framework Core e mecanismos de consulta e atualização.

---

## Worker.Payments

Integração do fluxo de consumo com a Inbox para detectar mensagens já processadas antes da execução da lógica de negócio.

---

# Critérios de Validação

A implementação será considerada concluída quando:

* uma mensagem recebida pela primeira vez for registrada como `PROCESSING` antes da execução do handler;
* o `EventId` da mensagem for persistido na Inbox;
* uma mensagem processada com sucesso for marcada como `PROCESSED`;
* o redelivery do mesmo `EventId` já processado não executar novamente a lógica de negócio;
* uma mensagem duplicada em estado `PROCESSED` resultar em ACK;
* uma falha durante o processamento marcar a mensagem como `FAILED`;
* uma mensagem em estado `FAILED` permanecer elegível para nova tentativa;
* uma nova tentativa de uma mensagem `FAILED` permitir sua transição para `PROCESSING` e posteriormente para `PROCESSED`;
* uma mensagem em `PROCESSING` dentro do timeout não ser processada simultaneamente por outra instância do Consumer;
* uma mensagem que permanecer em `PROCESSING` além do timeout configurado poder ter seu processamento reiniciado;
* o estado `PROCESSING` persistido ser visível para outras instâncias do Consumer enquanto o handler estiver em execução;
* o controle de duplicidade continuar funcionando após reinicializações do Worker;
* o comportamento permanecer compatível com Retry e Dead Letter Queue.

---

# Referências

* Enterprise Integration Patterns
* Microservices Patterns — Chris Richardson
* Idempotent Consumer Pattern
* Microsoft Architecture Guides

---

# Histórico

| Data       | Alteração                                                                																																		   |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 14/08/2026 | Criação da ADR-010 definindo a estratégia de Inbox Pattern do OrderFlow. 																																		   |
| 06/09/2026 | Evolução do Inbox Pattern com estados `PROCESSING`, `PROCESSED` e `FAILED`, claim persistente antes da execução do handler, recuperação por timeout e suporte ao processamento concorrente entre múltiplos Workers. |
