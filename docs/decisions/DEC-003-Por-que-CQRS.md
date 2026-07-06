# DEC-003 — Por que CQRS?

## Problema

Como organizar os casos de uso da aplicação de forma escalável, mantendo baixo acoplamento entre as camadas, alta coesão e facilidade de manutenção?

Em aplicações tradicionais, é comum concentrar toda a lógica de escrita, leitura, validação e orquestração em classes de serviço (`Application Services`). À medida que a aplicação cresce, essas classes tendem a acumular responsabilidades, tornando-se difíceis de manter e testar.

Era necessário definir uma estratégia que permitisse organizar a camada Application de forma mais modular, preparando o projeto para futuras integrações orientadas a eventos.

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
- ...
```

### Vantagens

- Estrutura simples.
- Menor quantidade de arquivos.
- Curva de aprendizado reduzida.

### Desvantagens

- Alto acoplamento.
- Classes grandes.
- Mistura de responsabilidades.
- Evolução mais difícil.
- Maior risco de conflitos em equipes maiores.
- Menor aderência aos princípios da Clean Architecture.

---

# Alternativa 2 — CQRS utilizando MediatR

Cada caso de uso é tratado individualmente.

Exemplo:

```text
CreateOrder

- Command
- Handler
- Validator
- Response
```

Cada funcionalidade passa a representar um Vertical Slice da aplicação.

### Vantagens

- Casos de uso independentes.
- Alta coesão.
- Baixo acoplamento.
- Melhor manutenção.
- Melhor testabilidade.
- Excelente integração com Pipeline Behaviors.
- Estrutura preparada para arquitetura orientada a eventos.

### Desvantagens

- Maior quantidade de arquivos.
- Curva de aprendizado inicial.
- Necessidade de maior disciplina arquitetural.

---

# Decisão

Foi adotado o padrão **CQRS** utilizando **MediatR**.

Cada caso de uso passou a possuir sua própria estrutura composta por:

- Command ou Query;
- Handler;
- Validator;
- Request/Response (quando aplicável).

Os casos de uso são organizados por **Feature**, seguindo a abordagem **Vertical Slice Architecture**.

Essa organização facilita a evolução da aplicação e reduz significativamente o acoplamento entre funcionalidades.

---

# Decisões complementares

Durante a implementação da camada Application foram adotadas algumas decisões arquiteturais para manter a consistência do projeto.

## ValidationBehavior

A validação ocorre automaticamente antes da execução do Handler.

Fluxo:

```text
Command

↓

ValidationBehavior

↓

Validator

↓

Handler
```

Dessa forma:

- o Handler não executa validações de entrada;
- o código permanece mais limpo;
- evita-se repetição de validações.

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

## Repositories

Os Handlers dependem exclusivamente de interfaces.

Exemplo:

```csharp
IOrderRepository
```

As implementações concretas pertencem à camada Infrastructure.

Essa decisão mantém a camada Application completamente desacoplada dos mecanismos de persistência.

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

Essa organização melhora a navegação pelo código e facilita a manutenção.

---

## Domain First

Os Handlers não implementam regras de negócio.

Seu papel é apenas:

- localizar o Aggregate;
- executar um comportamento do domínio;
- solicitar persistência.

Toda regra de negócio permanece encapsulada nas entidades do domínio.

---

## Repositório de Leitura

Foi criada a abstração:

```csharp
IOrderReadRepository
```

Essa interface possui responsabilidade exclusiva sobre operações de leitura.

Dessa forma:

- Commands utilizam repositórios do domínio;
- Queries utilizam repositórios especializados para leitura.

Essa separação permite evoluir os mecanismos de consulta de forma independente da escrita, mantendo aderência aos princípios do CQRS.

# Conclusão

A adoção do CQRS permitiu organizar a camada Application em pequenos casos de uso independentes, reduzindo o acoplamento entre funcionalidades e tornando a arquitetura mais preparada para evoluções futuras.

Além da separação entre escrita e leitura, essa decisão possibilitou a introdução de componentes como ValidationBehavior, UnitOfWork e Vertical Slice Architecture, estabelecendo uma base sólida para a implementação da camada Infrastructure e, posteriormente, da arquitetura orientada a eventos.

---

# Documentos relacionados

- ADR-004 — Adoção do CQRS
- Concepts/03-CQRS
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events

---

## Read Models

Foi adotada a estratégia de retornar modelos de leitura específicos para cada Query.

Exemplos:

- GetOrderByIdResponse
- GetOrdersResponse

Essa abordagem evita o vazamento das entidades do domínio para as camadas externas e permite que cada consulta retorne exatamente as informações necessárias para seu respectivo caso de uso.

Optou-se por manter DTOs independentes para cada Query, evitando abstrações prematuras. Caso surjam diversos modelos de leitura semelhantes, essa decisão poderá ser revisitada futuramente.