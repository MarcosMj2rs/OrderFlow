# Arquitetura Geral

Este diagrama representa a modelagem inicial do domínio do **OrderFlow**.

```mermaid
classDiagram

Entity <|-- Order
Entity <|-- OrderItem

Order "1" *-- "*" OrderItem

class Entity {
    +Guid Id
    +IReadOnlyCollection~IDomainEvent~ DomainEvents
    +ClearDomainEvents()
}

class Order {
    +Guid CustomerId
    +DateTime CreatedAt
    +OrderStatus Status
    +decimal TotalAmount
    +IReadOnlyCollection~OrderItem~ Items
    +AddItem()
    +RemoveItem()
    +ChangeItemQuantity()
    +Cancel()
    +Pay()
}

class OrderItem {
    +Guid ProductId
    +int Quantity
    +decimal UnitPrice
    +decimal Total
}

class IDomainEvent {
    +Guid EventId
    +DateTime OccurredAt
}

class DomainEvent {
    +Guid EventId
    +DateTime OccurredAt
}

IDomainEvent <|.. DomainEvent
DomainEvent <|-- OrderCreatedDomainEvent
DomainEvent <|-- OrderCancelledDomainEvent
DomainEvent <|-- OrderPaidDomainEvent