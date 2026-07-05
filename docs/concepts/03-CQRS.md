# CQRS (Command Query Responsibility Segregation)

## 1. Objetivo

Apresentar o padrão CQRS e sua aplicação no projeto OrderFlow.

O CQRS separa claramente operações de escrita (Commands) das operações de leitura (Queries), permitindo que cada uma evolua de forma independente.

---

## 2. Motivação

Em aplicações tradicionais, a mesma camada costuma ser responsável por:

- alterar dados;
- consultar dados;
- validar entradas;
- aplicar regras de negócio.

Com o crescimento da aplicação, essa abordagem tende a gerar alto acoplamento e classes cada vez maiores.

O CQRS organiza a aplicação em casos de uso independentes.

---

## 3. Conceito

CQRS significa:

**Command Query Responsibility Segregation**

O padrão divide a aplicação em dois tipos de operação.

### Commands

Representam intenções de alterar o estado do sistema.

Exemplos:

- CreateOrder
- CancelOrder
- PayOrder

Commands podem gerar Domain Events.

---

### Queries

Representam consultas.

Exemplos:

- GetOrderById
- GetOrders

Queries nunca alteram o estado do domínio.

---

## 4. Como aplicamos no OrderFlow

Cada caso de uso possui sua própria pasta.

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

Toda a lógica de orquestração fica concentrada no Handler.

As regras de negócio permanecem no Domain.

---

## 5. Fluxo

```text
Controller

↓

CreateOrderCommand

↓

ValidationBehavior

↓

CreateOrderCommandValidator

↓

CreateOrderCommandHandler

↓

Order (Domain)

↓

Repository

↓

UnitOfWork

↓

Response
```

---

## 6. Casos de uso implementados

Atualmente a camada Application possui os seguintes casos de uso:

### CreateOrder

Responsável pela criação de um novo pedido.

Fluxo:

- valida o Command;
- cria o Aggregate Order;
- persiste utilizando o Repository;
- confirma a operação através do Unit of Work.

---

### CancelOrder

Responsável pelo cancelamento de um pedido existente.

Fluxo:

- localiza o Aggregate;
- verifica sua existência;
- executa o comportamento Cancel();
- persiste utilizando o Unit of Work.

Observe que o Handler não altera diretamente propriedades da entidade.

Toda regra permanece encapsulada no Aggregate.

---

### Handler

Orquestra o caso de uso.

Não contém regras de negócio.

---

### Validator

Valida a entrada da aplicação.

Não substitui as validações do domínio.

---

## Validation Behavior

O projeto utiliza um Pipeline Behavior do MediatR responsável por executar automaticamente todos os Validators registrados antes da execução do Handler.

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

## 7. Benefícios

- Alta organização.
- Casos de uso independentes.
- Facilidade para testes.
- Baixo acoplamento.
- Melhor manutenção.
- Excelente integração com MediatR.

---

## 8. Desvantagens

- Maior quantidade de arquivos.
- Curva de aprendizado.

---

## 9. Boas práticas

- Um Handler por caso de uso.
- Commands não possuem regras de negócio.
- Validators apenas validam entrada.
- Handlers apenas orquestram.
- O domínio continua sendo responsável pelas regras.

---

## 10. Erros comuns

❌ Colocar regras de negócio no Handler.

❌ Executar consultas dentro do domínio.

❌ Duplicar validações do domínio na Application.

❌ Criar Handlers gigantes.

---

## 11. Conclusão

O CQRS permitiu organizar a camada Application em pequenos casos de uso independentes, mantendo o domínio isolado das responsabilidades de aplicação.

---

## 12. Documentos relacionados

- ADR-004-CQRS
- DEC-003-Por-que-CQRS
- 02-Domain-Driven-Design
- 07-Domain-Events

---

## Unit of Work

Foi criada a abstração IUnitOfWork.

Seu objetivo é desacoplar a camada Application do Entity Framework Core.

A Application apenas solicita:

```csharp
await _unitOfWork.SaveChangesAsync();
```

A implementação concreta será responsabilidade da Infrastructure.