# ADR-004 — Adoção do CQRS

## Status

**Aceita**

---

## Contexto

Durante a evolução do **OrderFlow**, tornou-se necessário organizar a camada **Application** de forma que cada caso de uso possuísse responsabilidades bem definidas.

Uma abordagem tradicional baseada em *Application Services* tende a concentrar operações de escrita, leitura, validação e orquestração em poucas classes, aumentando o acoplamento e dificultando a evolução da aplicação.

Era necessário adotar uma arquitetura que permitisse:

- separar operações de escrita das operações de leitura;
- organizar os casos de uso de forma independente;
- facilitar testes unitários;
- manter o domínio isolado das responsabilidades da camada Application;
- preparar a aplicação para futuras integrações orientadas a eventos.

---

## Decisão

Foi adotado o padrão **CQRS (Command Query Responsibility Segregation)** utilizando **MediatR**.

Cada caso de uso foi implementado seguindo o conceito de **Vertical Slice Architecture**, reunindo todos os componentes necessários para sua execução em uma única estrutura.

Cada Command ou Query possui seus próprios componentes, como:

- Command ou Query;
- Handler;
- Validator;
- Request/Response (quando aplicável).

Além disso, foram adotadas as seguintes diretrizes arquiteturais:

- Os Handlers possuem responsabilidade exclusiva de orquestração.
- As regras de negócio permanecem encapsuladas no domínio.
- A validação de entrada é executada pelo FluentValidation através do `ValidationBehavior`.
- A persistência é confirmada através da abstração `IUnitOfWork`, mantendo a camada Application desacoplada da infraestrutura.
- Operações de leitura utilizam um repositório especializado (`IOrderReadRepository`), separado dos repositórios do domínio.

---

## Justificativa

A adoção do CQRS proporciona uma organização mais adequada para aplicações orientadas a domínio.

Cada caso de uso representa uma unidade independente de comportamento, facilitando:

- manutenção;
- evolução;
- testes;
- localização do código;
- compreensão da aplicação.

A separação entre escrita e leitura também prepara naturalmente a solução para futuras implementações como:

- Entity Framework Core;
- Outbox Pattern;
- RabbitMQ;
- Event-Driven Architecture;
- Sagas;
- Consistência Eventual.

---

## Consequências

### Positivas

- Separação clara entre escrita e leitura.
- Organização por caso de uso (Vertical Slice Architecture).
- Alta coesão.
- Baixo acoplamento.
- Facilidade para testes unitários.
- Handlers pequenos e objetivos.
- Evolução independente entre escrita e leitura.
- Preparação para arquitetura orientada a eventos.

### Negativas

- Maior quantidade de arquivos.
- Curva de aprendizado para equipes sem experiência com CQRS.
- Necessidade de maior disciplina arquitetural durante o desenvolvimento.

---

## Estado da implementação

Ao término do Capítulo 4, a camada **Application** encontra-se completamente implementada.

### Commands

- CreateOrder
- CancelOrder
- PayOrder

### Queries

- GetOrderById
- GetOrders

### Componentes da camada Application

- ValidationBehavior
- MediatR
- FluentValidation
- IUnitOfWork
- IOrderReadRepository
- ApplicationDependencyInjection

Todos os casos de uso seguem a organização baseada em **Vertical Slice Architecture**, mantendo alta coesão e baixo acoplamento entre as funcionalidades.

As implementações concretas de persistência serão desenvolvidas no próximo capítulo, dedicado à camada **Infrastructure**.

---

## Alternativas consideradas

### Application Services tradicionais

**Vantagens**

- Estrutura inicial mais simples.
- Menor quantidade de arquivos.

**Desvantagens**

- Alto acoplamento.
- Crescimento excessivo das classes de serviço.
- Mistura de responsabilidades.
- Maior dificuldade de manutenção.
- Menor aderência ao modelo de domínio.

---

## Resultado

A adoção do CQRS mostrou-se adequada para os objetivos do OrderFlow.

Ao final da implementação da camada Application, a arquitetura apresenta:

- separação clara entre escrita e leitura;
- casos de uso independentes;
- domínio isolado da infraestrutura;
- validação centralizada;
- alta testabilidade;
- organização baseada em Features.

Essas decisões estabelecem uma base sólida para a implementação da camada **Infrastructure**, preservando os princípios da Clean Architecture e do Domain-Driven Design.

---

## Documentos relacionados

- Concepts/03-CQRS
- DEC-003-Por-que-CQRS
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events
- Concepts/04-Entity-Framework-Core