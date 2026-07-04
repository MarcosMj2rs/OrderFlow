# Domain-Driven Design (DDD)

## Objetivo

O OrderFlow utiliza os princípios do Domain-Driven Design para construir um domínio rico, consistente e orientado ao negócio.

---

# Aggregate

O Aggregate do sistema é representado por:

```text
Order
 └── OrderItem
```

A entidade `Order` é responsável por manter todas as invariantes do Aggregate.

---

# Aggregate Root

A única porta de entrada do Aggregate é a entidade `Order`.

Nenhuma alteração em `OrderItem` pode ocorrer sem passar pela `Order`.

---

# Entidades

O domínio possui atualmente duas entidades.

| Entidade | Responsabilidade |
|----------|------------------|
| Order | Representa o pedido e coordena todo o Aggregate. |
| OrderItem | Representa um item pertencente ao pedido. |

---

# Encapsulamento

As coleções do Aggregate nunca são expostas diretamente.

```csharp
private readonly List<OrderItem> _items;

public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
```

Essa abordagem impede alterações externas na coleção.

---

# Invariantes

O Aggregate garante as seguintes regras:

- Um pedido nasce com pelo menos um item.
- Não é possível remover o último item.
- Pedido cancelado não aceita alterações.
- O valor total do pedido é sempre recalculado automaticamente.

---

# Responsabilidades

## Order

- Criar itens
- Adicionar itens
- Remover itens
- Alterar quantidade
- Recalcular TotalAmount
- Controlar Status

## OrderItem

- Validar seus próprios dados
- Alterar quantidade
- Alterar preço
- Recalcular Total

---

# Benefícios

- Modelo rico
- Alta coesão
- Baixo acoplamento
- Encapsulamento
- Regras centralizadas