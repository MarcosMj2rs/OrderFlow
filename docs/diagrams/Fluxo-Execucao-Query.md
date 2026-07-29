HTTP GET
    │
    ▼
OrdersController
    │
    ▼
MediatR
    │
    ▼
Query
    │
    ▼
QueryHandler
    │
    ▼
ReadRepository
    │
    ▼
EF Core
    │
    ▼
SQL Server
    │
    ▼
AutoMapper
    │
    ▼
HTTP Response

___

flowchart TD

A[HTTP GET]

--> B[OrdersController]

--> C[MediatR]

--> D[GetOrdersQuery]

--> E[GetOrdersQueryHandler]

--> F[IOrderReadRepository]

--> G[OrderReadRepository]

--> H[SQL Server]

H --> G

G --> E

E --> C

C --> B

B --> I[AutoMapper]

I --> J[HTTP 200]