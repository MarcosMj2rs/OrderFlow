# DEC-004 — Por que SQL Server?

## Problema

O OrderFlow precisava definir um banco de dados relacional para suportar a persistência dos Aggregates do domínio.

A solução escolhida deveria:

- integrar-se naturalmente ao Entity Framework Core;
- suportar transações ACID;
- preservar a integridade referencial;
- permitir evolução controlada do esquema através de migrations;
- oferecer suporte às próximas etapas do projeto, como Outbox Pattern e Consistência Eventual.

Além disso, a tecnologia deveria reduzir a complexidade inicial do ambiente de desenvolvimento, permitindo que o foco permanecesse na arquitetura da aplicação.

---

# Alternativa 1 — PostgreSQL

O PostgreSQL é um banco de dados extremamente robusto e amplamente utilizado em sistemas distribuídos.

Também possui excelente integração com o Entity Framework Core e suporte completo a migrations, transações e recursos avançados.

Entretanto, como o objetivo do laboratório não é comparar bancos de dados, sua adoção não traria vantagens significativas em relação ao SQL Server neste momento.

---

# Alternativa 2 — SQLite

O SQLite simplificaria bastante o ambiente de desenvolvimento.

Por outro lado, apresenta limitações importantes quando comparado a bancos utilizados em produção, principalmente em relação a:

- concorrência;
- recursos de servidor;
- escalabilidade;
- compatibilidade com cenários corporativos.

Por esse motivo, foi descartado como banco principal do projeto.

---

# Alternativa 3 — SQL Server em Docker

Outra possibilidade seria executar o SQL Server em container.

Essa abordagem aumenta a portabilidade do ambiente, porém adiciona uma camada de configuração que não traz benefícios relevantes para os objetivos atuais do laboratório.

Como o foco desta etapa está na integração entre Domain, Entity Framework Core e banco de dados, optou-se por reduzir a complexidade operacional utilizando uma instalação local.

---

# Decisão

Foi adotado o **Microsoft SQL Server** executado localmente.

A aplicação acessa o banco exclusivamente através do Entity Framework Core, mantendo toda a persistência isolada na camada Infrastructure.

A Connection String permanece fora do código-fonte, sendo armazenada em **User Secrets** durante o desenvolvimento.

---

# Decisões Complementares

## Entity Framework Core

Toda comunicação com o banco ocorre através do Entity Framework Core.

Não existe acesso direto via SQL na camada Application ou Domain.

---

## Migrations

Toda evolução estrutural do banco deve ocorrer através de Migrations.

Foi criada a migration inicial (`InitialCreate`), responsável por gerar:

- tabela `Orders`;
- tabela `OrderItems`;
- índices;
- Foreign Keys;
- tabela `__EFMigrationsHistory`.

Alterações manuais no esquema devem ser evitadas para preservar o histórico do projeto.

---

## Change Tracking

As alterações do domínio são persistidas utilizando o Change Tracking do Entity Framework Core.

O fluxo adotado é:

```text
Buscar Aggregate

↓

Executar comportamento

↓

SaveChangesAsync()
```

Não é utilizada atualização explícita através de:

```csharp
_context.Update(entity);
```

---

## Model Snapshot

Foi adotado o mecanismo padrão do Entity Framework Core para controle da evolução do modelo.

O arquivo:

```text
OrderFlowDbContextModelSnapshot.cs
```

representa o estado atual do modelo e serve de base para geração das próximas migrations.

---

## Segurança

Durante o desenvolvimento, as credenciais do banco permanecem armazenadas em User Secrets.

Essa abordagem evita o versionamento de senhas e permite configurações distintas para diferentes ambientes.

---

## Permissões

Para permitir a aplicação das migrations durante o desenvolvimento, foi concedida ao usuário da aplicação a role:

```text
db_owner
```

Essa configuração é adequada apenas para ambientes de desenvolvimento.

Em ambientes produtivos, recomenda-se separar:

- usuário responsável por migrations;
- usuário utilizado pela aplicação.

---

# Resultado

A utilização do SQL Server proporcionou:

- integração transparente com o Entity Framework Core;
- persistência desacoplada do domínio;
- controle de evolução do banco através de migrations;
- suporte completo a transações e integridade referencial;
- ambiente preparado para a implementação futura do Outbox Pattern e da mensageria.

A decisão mostrou-se adequada aos objetivos do OrderFlow, fornecendo uma base consistente para os próximos capítulos do laboratório.