# DEC-003 — Por que CQRS?

## Problema

Como organizar os casos de uso da aplicação de forma escalável, mantendo baixo acoplamento entre as camadas, alta coesão e facilidade de manutenção?

Em aplicações tradicionais, é comum concentrar operações de escrita, leitura, validação e orquestração em classes de serviço (*Application Services*). À medida que a aplicação cresce, essas classes tendem a acumular responsabilidades, tornando-se difíceis de manter, testar e evoluir.

Era necessário definir uma estratégia que permitisse organizar a camada **Application** de forma modular, preparando o projeto para futuras integrações orientadas a eventos.

---

# Alternativa 1 — Application Services Tradicionais

Toda a lógica da aplicação ficaria concentrada em classes de serviço.

Exemplo:

```text
OrderService

- CreateOrder()
- CancelOrder()
- PayOrder()
- GetOrder()
- GetOrders()
```

## Vantagens

- Estrutura inicial simples.
- Menor quantidade de arquivos.
- Curva de aprendizado reduzida.

## Desvantagens

- Alto acoplamento.
- Classes grandes.
- Mistura de responsabilidades.
- Evolução mais difícil.
- Maior risco de conflitos em equipes maiores.
- Menor aderência aos princípios da Clean Architecture.

---

# Alternativa 2 — CQRS utilizando MediatR

Cada caso de uso passa a representar uma unidade independente da aplicação.

Exemplo:

```text
CreateOrder

- Command
- Handler
- Validator
- Response
```

Cada funcionalidade é organizada seguindo o conceito de **Vertical Slice Architecture**.

## Vantagens

- Casos de uso independentes.
- Alta coesão.
- Baixo acoplamento.
- Melhor manutenção.
- Excelente testabilidade.
- Integração natural com Pipeline Behaviors.
- Preparação para arquiteturas orientadas a eventos.

## Desvantagens

- Maior quantidade de arquivos.
- Curva de aprendizado inicial.
- Necessidade de maior disciplina arquitetural.

---

# Decisão

Foi adotado o padrão **CQRS**, utilizando **MediatR** como mecanismo de despacho dos casos de uso.

Cada caso de uso possui sua própria estrutura composta por:

- Command ou Query;
- Handler;
- Validator;
- Request/Response (quando aplicável).

Todos os casos de uso são organizados por **Feature**, seguindo a abordagem **Vertical Slice Architecture**.

Essa organização reduz o acoplamento entre funcionalidades, melhora a navegação pelo código e facilita a evolução da aplicação.

---

# Decisões complementares

Durante a implementação da camada **Application**, foram adotadas decisões arquiteturais adicionais para manter a consistência da solução.

## ValidationBehavior

A validação ocorre automaticamente antes da execução do Handler.

Fluxo:

```text
Command / Query

↓

ValidationBehavior

↓

Validator

↓

Handler
```

Dessa forma:

- o Handler não executa validações de entrada;
- evita-se repetição de código;
- mantém-se um pipeline consistente para todos os casos de uso.

---

## Unit of Work

Foi criada a abstração:

```csharp
IUnitOfWork
```

Seu objetivo é ocultar os detalhes de persistência da camada Application.

A Application apenas solicita:

```csharp
await _unitOfWork.SaveChangesAsync();
```

Sem conhecer:

- DbContext;
- Entity Framework Core;
- SQL Server.

---

## Repositórios

A arquitetura passou a utilizar dois tipos de repositório.

### IOrderRepository

Responsável pelas operações de escrita sobre o Aggregate `Order`.

É utilizado exclusivamente pelos Commands.

---

### IOrderReadRepository

Responsável pelas operações de leitura.

É utilizado exclusivamente pelas Queries.

Essa separação reforça a aplicação do CQRS e permite evoluir mecanismos de leitura e escrita de forma independente.

---

## Vertical Slice Architecture

Cada caso de uso possui todos os seus componentes reunidos em uma única pasta.

Exemplo:

```text
CreateOrder

- CreateOrderCommand
- CreateOrderCommandHandler
- CreateOrderCommandValidator
- CreateOrderResponse
```

Essa organização melhora a navegação pelo código, aumenta a coesão e reduz o acoplamento entre funcionalidades.

---

## Domain First

Os Handlers não implementam regras de negócio.

Seu papel é apenas:

- localizar o Aggregate;
- executar um comportamento do domínio;
- solicitar persistência;
- retornar o resultado.

Toda regra de negócio permanece encapsulada no domínio.

---

## Read Models

Foi adotada a estratégia de retornar modelos específicos para cada operação de leitura.

Exemplos:

- GetOrderByIdResponse
- GetOrdersResponse

Essa abordagem evita o vazamento das entidades do domínio para as camadas externas e permite que cada consulta retorne apenas as informações necessárias para seu respectivo caso de uso.

Optou-se por manter DTOs independentes, evitando abstrações prematuras.

---

# Resultado

Ao término da implementação da camada **Application**, o CQRS mostrou-se adequado aos objetivos do OrderFlow.

Foram implementados:

## Commands

- CreateOrder
- CancelOrder
- PayOrder

## Queries

- GetOrderById
- GetOrders

Além disso, a camada Application passou a contar com:

- MediatR;
- FluentValidation;
- ValidationBehavior;
- IUnitOfWork;
- IOrderRepository;
- IOrderReadRepository;
- Vertical Slice Architecture.

Essa decisão estabeleceu uma base sólida para o próximo capítulo do projeto, dedicado à implementação da camada **Infrastructure**.

---

# Conclusão

A adoção do CQRS permitiu organizar a camada **Application** em pequenos casos de uso independentes, reduzindo o acoplamento entre funcionalidades e aumentando significativamente a manutenibilidade da solução.

A arquitetura construída preserva os princípios da **Clean Architecture** e do **Domain-Driven Design**, preparando o OrderFlow para a implementação da persistência, mensageria e arquitetura orientada a eventos.

---

# Documentos relacionados

- ADR-004 — Adoção do CQRS
- Concepts/03-CQRS
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events