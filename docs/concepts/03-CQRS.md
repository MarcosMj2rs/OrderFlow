# CQRS (Command Query Responsibility Segregation)

# 1. Objetivo

Apresentar o padrão **CQRS (Command Query Responsibility Segregation)** e demonstrar como ele foi aplicado no projeto **OrderFlow**.

No OrderFlow, o CQRS organiza a camada **Application**, separando operações de escrita (**Commands**) das operações de leitura (**Queries**), permitindo que cada uma evolua de forma independente.

Essa separação aumenta a coesão dos casos de uso, reduz o acoplamento entre responsabilidades e prepara a aplicação para futuras arquiteturas distribuídas e orientadas a eventos.

---

# 2. Motivação

Em aplicações tradicionais, é comum encontrar uma única camada responsável por:

- alterar dados;
- consultar informações;
- validar entradas;
- aplicar regras de negócio;
- coordenar persistência.

Com o crescimento da aplicação, essas responsabilidades acabam concentradas em poucas classes, tornando a manutenção cada vez mais difícil.

O CQRS resolve esse problema organizando a aplicação em pequenos casos de uso independentes.

---

# 3. Conceito

CQRS significa:

**Command Query Responsibility Segregation**

O padrão divide a aplicação em dois grupos de operações.

## Commands

Representam intenções de alterar o estado do sistema.

No OrderFlow foram implementados:

- CreateOrder
- CancelOrder
- PayOrder

Os Commands:

- alteram o estado da aplicação;
- utilizam o Aggregate Root;
- executam regras de negócio;
- podem gerar Domain Events;
- confirmam alterações através do Unit of Work.

---

## Queries

Representam operações de leitura.

No OrderFlow foram implementadas:

- GetOrderById
- GetOrders

As Queries:

- nunca alteram o estado da aplicação;
- não executam regras de negócio;
- utilizam um repositório especializado de leitura (`IOrderReadRepository`);
- retornam modelos específicos de leitura (Read Models).

---

# 4. Arquitetura da camada Application

A camada Application foi organizada utilizando **Vertical Slice Architecture**.

Cada caso de uso possui sua própria estrutura.

Exemplo:

```text
Features
└── Orders
    └── Commands
        └── CreateOrder
            ├── CreateOrderCommand.cs
            ├── CreateOrderCommandHandler.cs
            ├── CreateOrderCommandValidator.cs
            └── CreateOrderResponse.cs
```

Cada pasta representa um único caso de uso da aplicação.

---

## Handler

O Handler possui responsabilidade exclusiva de orquestrar o caso de uso.

Ele:

- recebe o Command ou Query;
- utiliza os contratos necessários;
- coordena a execução;
- retorna o resultado.

O Handler **não contém regras de negócio**.

---

## Validator

Os Validators utilizam **FluentValidation**.

São responsáveis apenas por validar:

- obrigatoriedade;
- formato;
- consistência da entrada.

As regras de negócio permanecem exclusivamente no domínio.

---

## ValidationBehavior

O projeto utiliza um **Pipeline Behavior** do MediatR.

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

Caso a validação falhe, o Handler não é executado.

---

## Unit of Work

Foi criada a abstração:

```csharp
IUnitOfWork
```

Seu objetivo é desacoplar a camada Application do Entity Framework Core.

A Application apenas solicita:

```csharp
await _unitOfWork.SaveChangesAsync();
```

A implementação concreta será responsabilidade da camada Infrastructure.

---

## Repositories

O OrderFlow utiliza dois tipos de repositórios.

### IOrderRepository

Responsável pelas operações de escrita sobre o Aggregate `Order`.

É utilizado exclusivamente pelos Commands.

---

### IOrderReadRepository

Responsável pelas operações de leitura.

É utilizado exclusivamente pelas Queries.

Essa separação reforça a aplicação do padrão CQRS, permitindo que escrita e leitura evoluam de forma independente.

---

# 5. Fluxo de execução

## Commands

```text
Controller

↓

Command

↓

ValidationBehavior

↓

Validator

↓

Handler

↓

Aggregate Root

↓

Repository

↓

UnitOfWork

↓

Infrastructure

↓

SQL Server
```

---

## Queries

```text
Controller

↓

Query

↓

ValidationBehavior

↓

Validator

↓

Handler

↓

IOrderReadRepository

↓

Infrastructure

↓

SQL Server
```

Observe que apenas os Commands modificam o estado da aplicação.

As Queries possuem responsabilidade exclusiva de leitura.

---

# 6. Casos de uso implementados

## Commands

### CreateOrder

Responsável pela criação de um novo pedido.

---

### CancelOrder

Responsável pelo cancelamento de um pedido.

---

### PayOrder

Responsável pelo registro do pagamento de um pedido.

---

## Queries

### GetOrderById

Consulta um pedido completo por identificador.

---

### GetOrders

Lista os pedidos cadastrados retornando um modelo resumido contendo:

- OrderId
- CustomerId
- Status
- TotalAmount

---

# 7. Benefícios

- Separação entre escrita e leitura.
- Alta coesão.
- Baixo acoplamento.
- Casos de uso independentes.
- Excelente testabilidade.
- Excelente integração com MediatR.
- Facilidade para evolução da aplicação.
- Preparação para arquiteturas orientadas a eventos.

---

# 8. Desvantagens

- Maior quantidade de arquivos.
- Curva de aprendizado.
- Estrutura inicial mais sofisticada.

---

# 9. Boas práticas

- Um Handler por caso de uso.
- Um Validator por Command ou Query.
- Toda regra de negócio permanece no Domain.
- O Handler apenas orquestra.
- Toda persistência passa pelo UnitOfWork.
- Nunca acessar infraestrutura diretamente pelos Handlers.
- Organizar a Application por Features.
- Queries retornam Read Models, nunca entidades do domínio.

---

# 10. Erros comuns

❌ Colocar regras de negócio no Handler.

❌ Duplicar validações do domínio na Application.

❌ Fazer consultas dentro do Domain.

❌ Permitir que o Handler conheça o DbContext.

❌ Criar Services gigantes contendo vários casos de uso.

---

# 11. Commands x Queries

| Commands | Queries |
|----------|---------|
| Alteram estado da aplicação | Apenas leitura |
| Utilizam Aggregates | Utilizam Read Models |
| Executam regras de negócio | Não executam regras de negócio |
| Utilizam IOrderRepository | Utilizam IOrderReadRepository |
| Confirmam alterações | Não persistem dados |
| Podem gerar Domain Events | Nunca geram Domain Events |

Essa separação permite evoluir escrita e leitura de forma independente, conforme proposto pelo padrão CQRS.

---

# 12. Conclusão

Com a conclusão da camada **Application**, o OrderFlow passou a possuir uma implementação completa do padrão CQRS.

Foram implementados:

## Commands

- CreateOrder
- CancelOrder
- PayOrder

## Queries

- GetOrderById
- GetOrders

## Componentes da Application

- MediatR
- FluentValidation
- ValidationBehavior
- IUnitOfWork
- IOrderRepository
- IOrderReadRepository
- Vertical Slice Architecture

Toda a camada Application permanece desacoplada da infraestrutura, dependendo exclusivamente de abstrações.

O próximo capítulo abordará a implementação da camada **Infrastructure**, responsável pela persistência dos Aggregates, mapeamento das entidades, implementação dos repositórios e integração com o Entity Framework Core.

---

# 13. Documentos relacionados

- ADR-004 — CQRS
- DEC-003 — Por que CQRS?
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events
- Concepts/04-Entity-Framework-Core