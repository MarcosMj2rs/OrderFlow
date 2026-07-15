# SQL Server

## Objetivo

O SQL Server é o banco de dados relacional utilizado pelo **OrderFlow** para armazenar os Aggregates do domínio e garantir a persistência das informações da aplicação.

Neste projeto, o SQL Server representa a implementação concreta da camada de persistência, sendo acessado exclusivamente através do Entity Framework Core e dos repositórios definidos na Infrastructure.

Durante esta fase do laboratório, o foco é compreender a integração entre o modelo de domínio e um banco de dados relacional, preservando os princípios da Clean Architecture e do Domain-Driven Design.

---

# Papel na Arquitetura

O SQL Server pertence à camada de infraestrutura.

```text
Domain
    ↑
Application
    ↑
Infrastructure
    ├── Entity Framework Core
    └── SQL Server

WebApi
```

As camadas Domain e Application não possuem qualquer conhecimento sobre:

- SQL Server;
- Connection String;
- Tabelas;
- Chaves estrangeiras;
- Índices;
- Comandos SQL.

Toda comunicação ocorre através da Infrastructure.

---

# Ambiente de Desenvolvimento

Durante o desenvolvimento do OrderFlow foi adotada a utilização de um SQL Server instalado localmente.

Não foi utilizado container Docker para o banco de dados.

A configuração utilizada foi:

- SQL Server local;
- Banco de dados `OrderFlowDb`;
- Usuário dedicado para a aplicação (`orderflow_user`);
- Connection String armazenada em **User Secrets**.

Essa abordagem simplifica o ambiente de desenvolvimento e mantém o foco do laboratório na arquitetura da aplicação.

---

# Segurança das Configurações

A Connection String não permanece armazenada no repositório.

Durante o desenvolvimento, ela é carregada através do **User Secrets**, evitando que credenciais sejam versionadas juntamente com o código.

O `OrderFlowDbContext` obtém a conexão através da configuração da aplicação:

```csharp
configuration.GetConnectionString("OrderFlowDatabase");
```

Essa estratégia permite que diferentes ambientes utilizem configurações distintas sem necessidade de alteração no código.

---

# Entity Framework Core

O SQL Server é acessado através do Entity Framework Core.

O `OrderFlowDbContext` representa a sessão de trabalho entre a aplicação e o banco de dados.

Sua configuração é realizada utilizando:

```csharp
services.AddDbContext<OrderFlowDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

A camada Domain permanece totalmente desacoplada da tecnologia de persistência.

---

# Estrutura Inicial do Banco

A primeira migration criou a estrutura inicial composta por:

```text
Orders

OrderItems

__EFMigrationsHistory
```

As tabelas representam diretamente o Aggregate `Order` e sua entidade interna `OrderItem`.

---

# Relacionamentos

O relacionamento entre `Order` e `OrderItem` foi configurado como:

```text
Order

1

↓

N

OrderItem
```

A Foreign Key `OrderId` foi criada pelo Entity Framework Core utilizando uma **Shadow Property**, mantendo o domínio livre de propriedades voltadas exclusivamente para persistência.

---

# Tipos de Dados

Algumas decisões importantes foram tomadas durante o mapeamento.

## Identificadores

Todos os identificadores utilizam:

```text
uniqueidentifier
```

preservando os GUIDs gerados pelo próprio domínio.

---

## Datas

Datas são armazenadas como:

```text
datetime2
```

garantindo maior precisão quando comparado ao tipo `datetime`.

---

## Valores Monetários

Os valores monetários utilizam:

```text
decimal(18,2)
```

evitando problemas de precisão associados ao uso de tipos de ponto flutuante.

---

## Enumerações

As enumerações do domínio são persistidas como:

```text
int
```

através da conversão configurada no Entity Framework Core.

---

# Índices

A primeira versão do modelo criou índices para:

```text
OrderId

ProductId
```

O índice da Foreign Key melhora o desempenho das consultas envolvendo os itens do pedido.

O índice sobre `ProductId` prepara o modelo para futuras consultas relacionadas a produtos.

---

# Change Tracking

O SQL Server recebe apenas as alterações detectadas pelo Entity Framework Core.

O fluxo adotado pelo projeto é:

```text
Buscar Aggregate

↓

Executar comportamento

↓

SaveChangesAsync()

↓

SQL Server
```

Essa abordagem elimina a necessidade de comandos explícitos de atualização, permitindo que o EF Core determine automaticamente quais instruções SQL devem ser executadas.

---

# Migrations

Toda evolução estrutural do banco é realizada através de **Migrations**.

Cada migration representa uma alteração controlada do esquema do banco de dados.

Durante esta etapa do projeto foi criada a migration:

```text
InitialCreate
```

Ela foi responsável pela criação de toda a estrutura inicial do banco.

---

# Model Snapshot

Além das migrations, o Entity Framework Core mantém automaticamente um **Model Snapshot**.

Esse arquivo representa o estado atual do modelo conhecido pelo framework.

Sempre que uma nova migration é criada, o EF Core compara:

```text
Modelo Atual

↓

Model Snapshot
```

A partir dessa comparação são geradas apenas as alterações necessárias.

Essa estratégia elimina a necessidade de analisar todas as migrations anteriores durante a evolução do banco.

---

# Permissões

Durante a criação da primeira migration aplicada ao banco foi necessário conceder permissões adequadas ao usuário da aplicação.

Para o ambiente de desenvolvimento foi utilizada a role:

```text
db_owner
```

permitindo que o Entity Framework Core executasse operações de criação de tabelas, índices e chaves.

Em ambientes de produção, a recomendação é separar:

- usuário responsável pela execução de migrations;
- usuário utilizado pela aplicação em tempo de execução.

Essa abordagem reduz privilégios e aumenta a segurança do ambiente.

---

# Benefícios da abordagem adotada

A estratégia utilizada no OrderFlow proporciona:

- persistência desacoplada do domínio;
- evolução controlada do banco de dados;
- versionamento do esquema através de migrations;
- utilização de configurações por ambiente;
- maior segurança das credenciais;
- integração transparente com o Entity Framework Core;
- preparação para Outbox Pattern e mensageria.

---

# Conclusão

O SQL Server foi incorporado ao OrderFlow como tecnologia de persistência da camada Infrastructure, permanecendo completamente isolado das camadas de negócio.

A utilização conjunta de Entity Framework Core, Migrations e User Secrets estabelece uma base sólida para a evolução da aplicação, permitindo que os próximos capítulos implementem RabbitMQ, Outbox Pattern, Inbox Pattern e Consistência Eventual sem alterações estruturais na persistência.