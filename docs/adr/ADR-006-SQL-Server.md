# ADR-006 — SQL Server

## Status

**Aceita**

---

# Contexto

O OrderFlow necessitava de um banco de dados relacional capaz de armazenar os Aggregates do domínio de forma consistente, suportando transações, integridade referencial e integração completa com o Entity Framework Core.

Além disso, a solução deveria atender aos seguintes objetivos:

- suportar o modelo rico de domínio;
- permitir evolução controlada do esquema do banco;
- oferecer excelente integração com o Entity Framework Core;
- preparar a arquitetura para Outbox Pattern;
- possibilitar futura integração com RabbitMQ utilizando consistência eventual.

---

# Decisão

Foi adotado o **Microsoft SQL Server** como banco de dados oficial do projeto.

Durante o desenvolvimento, o SQL Server será executado localmente na máquina do desenvolvedor.

Não será utilizado container Docker para o banco de dados nesta fase do laboratório.

A persistência será realizada exclusivamente através do Entity Framework Core.

---

# Justificativa

O SQL Server apresenta excelente integração com o Entity Framework Core, oferecendo suporte completo para:

- Migrations;
- Change Tracking;
- Transações;
- Constraints;
- Índices;
- Foreign Keys;
- Tipos de dados avançados.

Além disso, trata-se de uma tecnologia amplamente utilizada em ambientes corporativos e adequada ao objetivo do laboratório.

---

# Decisões Arquiteturais

Durante a implementação foram adotadas as seguintes decisões.

## Persistência isolada na Infrastructure

O SQL Server pertence exclusivamente à camada Infrastructure.

As camadas Domain e Application não possuem qualquer conhecimento sobre:

- banco de dados;
- Connection String;
- SQL;
- tabelas;
- índices;
- chaves estrangeiras.

Toda comunicação ocorre através do Entity Framework Core.

---

## SQL Server Local

Durante o desenvolvimento foi adotado um SQL Server instalado localmente.

Essa decisão reduz a complexidade inicial do ambiente e permite concentrar o estudo na arquitetura da aplicação e na integração com o Entity Framework Core.

---

## User Secrets

A Connection String não permanece armazenada no código-fonte.

Foi adotado o uso de **User Secrets** durante o desenvolvimento para evitar o versionamento de credenciais.

A Infrastructure obtém a conexão através da configuração da aplicação:

```csharp
configuration.GetConnectionString("OrderFlowDatabase");
```

Essa abordagem facilita a utilização de diferentes configurações entre desenvolvimento, homologação e produção.

---

## Migrations

Toda alteração estrutural do banco deverá ocorrer através de Migrations.

Foi criada a primeira migration:

```text
InitialCreate
```

responsável por criar:

- Orders;
- OrderItems;
- índices;
- Foreign Keys;
- tabela `__EFMigrationsHistory`.

Alterações manuais no esquema do banco devem ser evitadas para manter o histórico consistente.

---

## Model Snapshot

Foi adotado o mecanismo padrão de controle de modelo do Entity Framework Core.

O arquivo:

```text
OrderFlowDbContextModelSnapshot.cs
```

representa o estado atual do modelo conhecido pelo framework e serve de base para geração das próximas migrations.

Esse arquivo não deve ser alterado manualmente.

---

## Tipos de Dados

Algumas decisões importantes foram adotadas durante o mapeamento.

### Identificadores

Todos os identificadores utilizam:

```text
uniqueidentifier
```

preservando os GUIDs gerados pelo domínio.

---

### Datas

As datas utilizam:

```text
datetime2
```

oferecendo maior precisão e melhor compatibilidade com versões recentes do SQL Server.

---

### Valores Monetários

Os valores monetários utilizam:

```text
decimal(18,2)
```

evitando perdas de precisão inerentes aos tipos de ponto flutuante.

---

### Enumerações

Os enums do domínio são persistidos como:

```text
int
```

através da conversão configurada no Entity Framework Core.

---

## Integridade Referencial

O relacionamento entre `Order` e `OrderItem` é protegido por Foreign Key.

Foi adotado:

```text
DeleteBehavior.Cascade
```

garantindo que, em uma eventual remoção física do Aggregate, seus itens também sejam removidos.

O cancelamento de pedidos continua sendo uma regra de negócio implementada no domínio e não representa exclusão física.

---

## Permissões

Durante a criação inicial da estrutura do banco foi necessário conceder permissões adequadas ao usuário da aplicação.

Para o ambiente de desenvolvimento foi utilizada a role:

```text
db_owner
```

Essa decisão facilita a execução das migrations durante o desenvolvimento.

Em ambientes produtivos, recomenda-se utilizar contas distintas para:

- execução de migrations;
- execução da aplicação.

Essa separação reduz privilégios e aumenta a segurança do ambiente.

---

# Consequências

A adoção do SQL Server proporciona:

- banco relacional robusto;
- integração nativa com Entity Framework Core;
- controle de evolução através de migrations;
- integridade referencial;
- suporte a transações;
- preparação para Outbox Pattern;
- facilidade de manutenção do esquema do banco.

Como consequência, toda evolução estrutural passa a ser controlada pelo Entity Framework Core, eliminando alterações manuais diretamente no banco.

---

# Estado da Implementação

Até este momento foram implementados:

- SQL Server local;
- banco `OrderFlowDb`;
- configuração via User Secrets;
- integração com Entity Framework Core;
- primeira migration (`InitialCreate`);
- criação automática das tabelas;
- criação dos índices;
- criação das Foreign Keys;
- aplicação da migration utilizando `database update`.

Toda a persistência encontra-se operacional.

---

# Alternativas Consideradas

Foram consideradas:

- PostgreSQL;
- SQLite;
- SQL Server em Docker.

Para os objetivos atuais do laboratório, a utilização do SQL Server instalado localmente apresentou menor complexidade operacional e excelente integração com o Entity Framework Core.

A utilização de Docker para o banco poderá ser avaliada em etapas futuras do projeto, caso exista necessidade de ambientes totalmente containerizados.

---

# Resultado

O SQL Server foi adotado como solução oficial de persistência do OrderFlow, permanecendo completamente isolado na camada Infrastructure.

Sua integração com o Entity Framework Core fornece uma base sólida para a evolução do laboratório, permitindo a implementação das próximas funcionalidades relacionadas à mensageria, Outbox Pattern, Inbox Pattern, Idempotência e Consistência Eventual.

---

# Documentos Relacionados

- 04-Entity-Framework-Core.md
- 05-SQL-Server.md
- ADR-005-Entity-Framework-Core.md
- DEC-004-Por-que-SQL-Server.md