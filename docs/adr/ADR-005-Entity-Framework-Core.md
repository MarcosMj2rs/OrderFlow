# ADR-005 — Entity Framework Core

## Status

**Aceita**

---

# Contexto

A camada de persistência do OrderFlow precisava atender aos seguintes objetivos:

- manter o domínio completamente desacoplado da infraestrutura;
- preservar os princípios da Clean Architecture;
- respeitar o modelo rico de domínio definido pelo DDD;
- suportar Aggregates com comportamento encapsulado;
- permitir evolução controlada do banco de dados;
- oferecer integração simples com SQL Server;
- preparar a arquitetura para Outbox Pattern e mensageria.

Além disso, o projeto utiliza um Aggregate Root (`Order`) contendo uma entidade interna (`OrderItem`), exigindo que o mecanismo de persistência respeite o encapsulamento do modelo.

---

# Decisão

Foi adotado o **Entity Framework Core** como ORM oficial do projeto.

Toda a persistência permanece concentrada na camada **Infrastructure**, utilizando:

- DbContext;
- Fluent API;
- Repositories;
- Unit of Work;
- Migrations.

Nenhuma entidade do domínio possui dependência do Entity Framework Core.

---

# Justificativa

O Entity Framework Core oferece recursos que permitem mapear modelos ricos de domínio sem comprometer sua modelagem.

Durante esta implementação foram utilizados recursos como:

- Fluent API;
- Backing Fields;
- Shadow Properties;
- Change Tracking;
- Migrations;
- Model Snapshot.

Esses recursos permitem que o domínio permaneça orientado ao comportamento, enquanto a Infrastructure assume toda a responsabilidade pela persistência.

---

# Decisões Arquiteturais

Durante a implementação foram adotadas as seguintes decisões.

## Domain não conhece EF Core

O Domain permanece completamente isolado.

Não foram utilizados:

- Data Annotations;
- atributos de mapeamento;
- DbContext;
- DbSet;
- classes do Entity Framework Core.

Toda configuração de persistência foi realizada através da Fluent API.

---

## Utilização da Fluent API

Todo o mapeamento do modelo foi centralizado em:

```text
Persistence
└── Configurations
    ├── OrderConfiguration
    └── OrderItemConfiguration
```

Essa abordagem elimina o acoplamento entre domínio e persistência.

---

## Aggregate Root

O Aggregate Root continua sendo exclusivamente:

```text
Order
```

A entidade:

```text
OrderItem
```

não possui repositório próprio e não pode ser manipulada independentemente.

Todas as alterações passam obrigatoriamente pelo Aggregate.

---

## Change Tracking

Foi decidido utilizar o Change Tracking nativo do Entity Framework Core.

O fluxo oficial do projeto é:

```text
Buscar Aggregate

↓

Executar comportamento

↓

SaveChangesAsync()
```

Essa estratégia elimina a necessidade de atualização explícita das entidades.

---

## Não utilizar Update()

Foi decidido não utilizar:

```csharp
_context.Update(entity);
```

Essa decisão evita problemas relacionados a:

- entidades detached;
- atualização de grafos completos;
- alterações indevidas em coleções;
- geração de comandos SQL desnecessários;
- perda de controle sobre o Aggregate.

As entidades são sempre recuperadas rastreadas pelo contexto.

---

## Backing Field

A coleção interna do Aggregate permanece encapsulada.

```csharp
private readonly List<OrderItem> _items = [];
```

O Entity Framework Core foi configurado para utilizar o campo privado como armazenamento da navegação.

A aplicação continua acessando apenas:

```csharp
IReadOnlyCollection<OrderItem>
```

---

## Shadow Property

A Foreign Key `OrderId` não faz parte do domínio.

Ela foi criada como Shadow Property pelo Entity Framework Core, mantendo a entidade `OrderItem` livre de propriedades destinadas exclusivamente ao banco de dados.

---

## Read Repository

Foi adotada a separação entre escrita e leitura.

Commands utilizam:

```text
OrderRepository
```

Queries utilizam:

```text
OrderReadRepository
```

O repositório de leitura realiza projeções diretamente para DTOs utilizando `AsNoTracking()`.

---

## Unit of Work

Foi criada uma implementação própria de `IUnitOfWork`.

Sua responsabilidade é centralizar a confirmação das alterações através de:

```csharp
SaveChangesAsync()
```

Essa decisão prepara a arquitetura para a implementação futura do Outbox Pattern.

---

## Migrations

A evolução do banco de dados passa obrigatoriamente por Migrations.

Foi criada a migration inicial:

```text
InitialCreate
```

responsável pela criação da estrutura inicial do banco.

---

## Model Snapshot

Foi adotado o mecanismo padrão do Entity Framework Core para controle da evolução do modelo.

O arquivo:

```text
OrderFlowDbContextModelSnapshot.cs
```

representa o estado atual conhecido pelo framework e serve como base para comparação durante a criação de novas migrations.

---

# Consequências

A adoção do Entity Framework Core proporciona:

- domínio desacoplado da persistência;
- manutenção do modelo rico de domínio;
- evolução controlada do banco de dados;
- menor quantidade de código SQL manual;
- suporte ao encapsulamento dos Aggregates;
- integração transparente com SQL Server;
- preparação para Outbox Pattern e mensageria.

Como consequência, a Infrastructure passa a ser responsável por toda a persistência da aplicação, preservando a independência das camadas Domain e Application.

---

# Estado da Implementação

Até este momento foram implementados:

- OrderFlowDbContext;
- Fluent API;
- OrderConfiguration;
- OrderItemConfiguration;
- OrderRepository;
- OrderReadRepository;
- UnitOfWork;
- InfrastructureDependencyInjection;
- User Secrets;
- Migration inicial;
- Banco de dados criado via Entity Framework Core.

Toda a persistência encontra-se funcional e integrada ao SQL Server.

---

# Alternativas Consideradas

Foram consideradas:

- Dapper;
- ADO.NET;
- SQL manual.

Essas alternativas exigiriam maior esforço de implementação para suportar:

- Change Tracking;
- Migrations;
- evolução do modelo;
- mapeamento de Aggregates;
- integração com Outbox Pattern.

Para os objetivos do laboratório, o Entity Framework Core apresentou melhor equilíbrio entre produtividade, flexibilidade e aderência ao modelo arquitetural adotado.

---

# Resultado

O Entity Framework Core foi incorporado ao OrderFlow preservando integralmente os princípios da Clean Architecture e do Domain-Driven Design.

O domínio permanece desacoplado da persistência, enquanto a Infrastructure concentra toda a responsabilidade pela comunicação com o SQL Server.

Essa decisão estabelece uma base sólida para os próximos capítulos do laboratório, especialmente RabbitMQ, Outbox Pattern, Inbox Pattern, Idempotência e Consistência Eventual.

---

# Documentos Relacionados

- 04-Entity-Framework-Core.md
- 05-SQL-Server.md
- ADR-002-Clean-Architecture.md
- ADR-003-Domain-Driven-Design.md
- ADR-004-CQRS.md
- DEC-004-Por-que-SQL-Server.md