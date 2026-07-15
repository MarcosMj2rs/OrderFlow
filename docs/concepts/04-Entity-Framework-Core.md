# Entity Framework Core

## Objetivo

O Entity Framework Core (EF Core) é o ORM (Object-Relational Mapper) utilizado pelo **OrderFlow** para realizar a persistência dos Aggregates do domínio no SQL Server.

Neste projeto, o EF Core é utilizado de forma alinhada aos princípios de **Domain-Driven Design (DDD)** e **Clean Architecture**, mantendo o domínio completamente desacoplado dos detalhes de infraestrutura.

A principal responsabilidade do EF Core é transformar os objetos do domínio em estruturas relacionais, preservando o comportamento e as invariantes definidas pelas entidades.

---

# Papel na Arquitetura

Dentro do OrderFlow, o EF Core pertence exclusivamente à camada **Infrastructure**.

```text
Domain
    ↑
Application
    ↑
Infrastructure
    ├── Entity Framework Core
    ├── SQL Server
    └── Repositories

WebApi
```

O Domain não conhece:

- Entity Framework Core
- DbContext
- DbSet
- Migrations
- SQL Server
- Fluent API

Toda configuração de persistência fica concentrada na Infrastructure.

---

# DbContext

O ponto central da persistência é o `OrderFlowDbContext`.

Sua responsabilidade é:

- representar a sessão de trabalho com o banco;
- controlar o Change Tracker;
- aplicar os mapeamentos Fluent API;
- coordenar a persistência das entidades.

O projeto utiliza:

```csharp
public DbSet<Order> Orders => Set<Order>();
```

Não existe:

```csharp
DbSet<OrderItem>
```

Essa decisão reforça que **OrderItem não é um Aggregate Root**, sendo manipulado exclusivamente através do Aggregate `Order`.

---

# Fluent API

Todo o mapeamento do modelo é realizado através da Fluent API.

Foram criadas as seguintes configurações:

```text
Persistence
└── Configurations
    ├── OrderConfiguration
    └── OrderItemConfiguration
```

Essa abordagem evita a utilização de atributos de persistência nas entidades do domínio, preservando o desacoplamento entre Domain e Infrastructure.

---

# Mapeamento do Aggregate

O Aggregate `Order` é persistido respeitando sua estrutura de domínio.

Foram configurados:

- chave primária (`Id`);
- enumeração (`OrderStatus`);
- precisão de valores monetários (`decimal(18,2)`);
- relacionamento entre `Order` e `OrderItem`;
- comportamento de exclusão (`Cascade`);
- exclusão dos Domain Events do modelo persistente.

---

# Backing Field

O Aggregate encapsula sua coleção de itens:

```csharp
private readonly List<OrderItem> _items = [];

public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
```

O EF Core foi configurado para utilizar o campo privado como armazenamento da navegação, preservando o encapsulamento da entidade.

Dessa forma:

- a aplicação não possui acesso à lista mutável;
- somente o Aggregate altera sua coleção;
- o EF Core consegue materializar corretamente os objetos durante consultas.

Essa abordagem permite manter o modelo rico de domínio sem comprometer a persistência.

---

# Shadow Property

A entidade `OrderItem` não possui a propriedade:

```csharp
OrderId
```

Mesmo assim, o banco possui a coluna correspondente.

Isso ocorre porque a Foreign Key foi configurada através de uma **Shadow Property**, criada internamente pelo EF Core.

Essa decisão permite que o domínio permaneça limpo, sem propriedades cuja única finalidade seria atender ao banco de dados.

---

# Change Tracking

Uma das decisões arquiteturais mais importantes do projeto é utilizar o **Change Tracking** do EF Core.

O fluxo adotado é:

```text
Buscar Aggregate

↓

Executar comportamento do domínio

↓

SaveChangesAsync()
```

Exemplo:

```csharp
var order = await repository.GetByIdAsync(id, cancellationToken);

order.Pay();

await unitOfWork.SaveChangesAsync(cancellationToken);
```

O EF Core detecta automaticamente quais propriedades foram alteradas e gera os comandos SQL necessários.

---

# Atualizações do Aggregate

O projeto deliberadamente **não utiliza**:

```csharp
_context.Update(order);
```

Essa decisão evita problemas relacionados a:

- entidades detached;
- atualização de grafos completos;
- alterações indevidas em coleções;
- geração de INSERTs e UPDATEs desnecessários;
- conflitos no Change Tracker.

Todo comportamento deve ocorrer sobre entidades rastreadas pelo contexto.

---

# Repositórios

Foram implementados dois tipos de repositório.

## Escrita

```text
OrderRepository
```

Responsável por:

- obter Aggregates rastreados;
- adicionar novos Aggregates;
- remover Aggregates;
- participar da unidade de trabalho.

---

## Leitura

```text
OrderReadRepository
```

Responsável por:

- consultas;
- projeções;
- DTOs;
- utilização de `AsNoTracking()`.

Queries nunca retornam entidades do domínio.

---

# AsNoTracking

As consultas de leitura utilizam:

```csharp
AsNoTracking()
```

Essa configuração reduz o consumo de memória e melhora o desempenho, uma vez que não existe necessidade de rastrear entidades destinadas apenas à consulta.

Essa estratégia é utilizada exclusivamente nos Read Repositories.

---

# Unit of Work

O projeto utiliza uma implementação própria de `IUnitOfWork`.

Sua responsabilidade é centralizar a chamada para:

```csharp
SaveChangesAsync()
```

Nenhum repositório executa persistência diretamente.

Essa separação será fundamental para a implementação futura do **Outbox Pattern**, permitindo que alterações do domínio e mensagens de integração sejam persistidas na mesma transação.

---

# Migrations

As migrations são mantidas dentro da Infrastructure.

```text
Persistence
└── Migrations
```

A primeira migration (`InitialCreate`) gerou:

- tabela `Orders`;
- tabela `OrderItems`;
- Foreign Key entre as tabelas;
- índices;
- conversão do enum para inteiro;
- colunas monetárias com precisão;
- Shadow Property (`OrderId`).

---

# Model Snapshot

Além da migration, o EF Core mantém automaticamente o arquivo:

```text
OrderFlowDbContextModelSnapshot.cs
```

Esse arquivo representa o estado atual do modelo conhecido pelo EF Core.

Durante a criação de novas migrations, o framework compara:

```text
Modelo Atual

↓

Model Snapshot
```

A partir dessa comparação são identificadas as diferenças que originam a próxima migration.

O Snapshot não representa histórico de alterações e não deve ser editado manualmente.

---

# Benefícios da abordagem adotada

A estratégia utilizada no OrderFlow proporciona:

- domínio desacoplado da persistência;
- preservação das regras de negócio;
- utilização correta do Aggregate Root;
- encapsulamento das coleções;
- melhor controle sobre Change Tracking;
- separação entre leitura e escrita (CQRS);
- facilidade de evolução do modelo;
- integração natural com Outbox Pattern e mensageria.

---

# Conclusão

O Entity Framework Core foi incorporado ao OrderFlow respeitando integralmente os princípios definidos para o projeto.

A persistência permanece isolada na camada Infrastructure, enquanto o domínio continua responsável exclusivamente pelas regras de negócio.

As decisões adotadas durante esta etapa estabelecem uma base sólida para os próximos capítulos, especialmente para a implementação do RabbitMQ, Outbox Pattern, Inbox Pattern e Consistência Eventual.