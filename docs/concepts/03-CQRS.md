# CQRS (Command Query Responsibility Segregation)

## 1. Objetivo

Apresentar o padrão **CQRS (Command Query Responsibility Segregation)** e demonstrar como ele foi aplicado no projeto **OrderFlow**.

O CQRS organiza a camada **Application** separando operações de escrita (**Commands**) das operações de leitura (**Queries**), permitindo que cada uma evolua de forma independente.

Essa separação aumenta a coesão dos casos de uso, reduz o acoplamento entre responsabilidades e facilita a manutenção da aplicação.

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

Exemplos:

- CreateOrder
- CancelOrder
- PayOrder

Commands podem gerar Domain Events.

---

## Queries

Representam consultas.

Exemplos:

- GetOrderById
- GetOrders

Queries nunca modificam o estado do domínio.

---

# 4. Arquitetura da camada Application

No OrderFlow a camada Application foi organizada seguindo o conceito de **Vertical Slice Architecture**.

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

- recebe o Command;
- utiliza o Aggregate;
- solicita persistência;
- retorna o resultado.

O Handler **não contém regras de negócio**.

---

## Validator

Os Validators utilizam FluentValidation.

Eles validam apenas:

- obrigatoriedade;
- formato;
- consistência da entrada.

As regras de negócio continuam pertencendo ao domínio.

---

## ValidationBehavior

O projeto utiliza um **Pipeline Behavior** do MediatR.

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

## Repository

Os Handlers dependem exclusivamente de interfaces.

Exemplo:

```csharp
IOrderRepository
```

Nenhum Handler conhece:

- DbContext
- Entity Framework Core
- SQL Server

---

# 5. Como aplicamos no OrderFlow

O fluxo de um Command segue sempre o mesmo padrão.

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

Persistência
```

Observe que o domínio permanece completamente isolado da infraestrutura.

---

# 6. Casos de uso implementados

Atualmente a camada Application possui os seguintes Commands.

## CreateOrder

Responsável pela criação de um novo pedido.

Fluxo:

- valida o Command;
- cria o Aggregate;
- persiste utilizando Repository;
- confirma através do UnitOfWork.

---

## CancelOrder

Responsável pelo cancelamento de um pedido.

Fluxo:

- localiza o Aggregate;
- verifica sua existência;
- executa `Cancel()`;
- confirma através do UnitOfWork.

---

## PayOrder

Responsável pelo pagamento de um pedido.

Fluxo:

- localiza o Aggregate;
- verifica sua existência;
- executa `Pay()`;
- confirma através do UnitOfWork.

---

# 7. Benefícios

- Separação entre escrita e leitura.
- Alta coesão.
- Baixo acoplamento.
- Casos de uso independentes.
- Facilidade para testes.
- Excelente integração com MediatR.
- Facilidade para evolução da aplicação.

---

# 8. Desvantagens

- Maior quantidade de arquivos.
- Curva de aprendizado.
- Estrutura inicial mais sofisticada.

---

# 9. Boas práticas

- Um Handler por caso de uso.
- Um Validator por Command.
- Toda regra de negócio permanece no Domain.
- O Handler apenas orquestra.
- Toda persistência passa pelo UnitOfWork.
- Nunca acessar infraestrutura diretamente pelos Handlers.
- Organizar a Application por Features.

---

# 10. Erros comuns

❌ Colocar regras de negócio no Handler.

❌ Duplicar validações do domínio na Application.

❌ Fazer consultas dentro do Domain.

❌ Permitir que o Handler conheça o DbContext.

❌ Criar Services gigantes contendo vários casos de uso.

---

# 11. Conclusão

A utilização de CQRS permitiu organizar a camada Application em pequenos casos de uso independentes, mantendo clara a separação entre regras de negócio, orquestração e infraestrutura.

Até o momento, o OrderFlow implementa toda a infraestrutura necessária para Commands, incluindo ValidationBehavior, UnitOfWork, Handlers, Validators e integração com o domínio.

As próximas etapas evoluirão a camada de leitura através das Queries e, posteriormente, a infraestrutura responsável pela persistência e mensageria.

---

# 12. Documentos relacionados

- ADR-004-CQRS
- DEC-003-Por-que-CQRS
- Concepts/02-Domain-Driven-Design
- Concepts/07-Domain-Events
- Concepts/04-Entity-Framework-Core