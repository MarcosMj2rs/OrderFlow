# ADR-004 — Adoção do CQRS

## Status

**Aceita**

---

## Contexto

À medida que o domínio do OrderFlow evolui, tornou-se necessário organizar a camada Application de forma que cada caso de uso possuísse responsabilidades bem definidas.

Uma abordagem tradicional baseada em Services tende a concentrar operações de escrita, leitura, validação e orquestração em poucas classes, aumentando o acoplamento e dificultando a evolução da aplicação.

Era necessário adotar uma arquitetura que permitisse:

- separar operações de escrita das operações de leitura;
- organizar os casos de uso de forma independente;
- facilitar testes unitários;
- manter o domínio isolado das responsabilidades de aplicação;
- preparar a aplicação para futuras integrações orientadas a eventos.

---

## Decisão

Foi adotado o padrão **CQRS (Command Query Responsibility Segregation)** utilizando **MediatR**.

Cada caso de uso é implementado como um **Vertical Slice**, contendo todos os componentes necessários para sua execução.

A estrutura padrão de um Command é composta por:

- Command;
- Handler;
- Validator;
- Request/Response (quando aplicável).

Todos os casos de uso são organizados por **Feature**, evitando estruturas centralizadas por tipo de arquivo.

Além disso, foram adotadas as seguintes diretrizes arquiteturais:

- Os Handlers possuem responsabilidade exclusiva de orquestração.
- As regras de negócio permanecem encapsuladas no domínio.
- A validação de entrada é executada pelo FluentValidation através de um `ValidationBehavior`.
- A persistência é confirmada através da abstração `IUnitOfWork`, mantendo a camada Application desacoplada da infraestrutura.

---

## Justificativa

A adoção do CQRS oferece uma organização mais adequada para aplicações orientadas a domínio.

Cada caso de uso passa a representar uma unidade independente de negócio, facilitando:

- manutenção;
- evolução;
- testes;
- localização do código;
- compreensão da aplicação.

Essa abordagem também prepara naturalmente a solução para futuras implementações como Outbox Pattern, RabbitMQ, Sagas e Event-Driven Architecture.

---

## Consequências

### Positivas

- Separação clara entre escrita e leitura.
- Organização por caso de uso (Vertical Slice).
- Alta coesão.
- Baixo acoplamento.
- Facilidade para testes unitários.
- Handlers pequenos e objetivos.
- Melhor preparação para arquitetura orientada a eventos.

### Negativas

- Maior quantidade de arquivos.
- Curva de aprendizado para equipes sem experiência com CQRS.
- Maior disciplina arquitetural durante o desenvolvimento.

---

## Estado atual da implementação

Até o momento, foram implementados os seguintes componentes da camada Application:

### Infraestrutura

- ValidationBehavior
- IUnitOfWork
- ApplicationDependencyInjection

### Commands

- CreateOrder
- CancelOrder
- PayOrder

As Queries serão implementadas na próxima etapa da evolução do projeto.

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
- Menor escalabilidade arquitetural.

---

## Documentos relacionados

- Concepts/03-CQRS
- DEC-003-Por-que-CQRS
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events