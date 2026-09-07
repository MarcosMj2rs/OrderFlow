# DEC-009 — Por que Idempotência?

## Contexto

O OrderFlow utiliza comunicação assíncrona por meio do RabbitMQ.

Nesse modelo, uma mensagem pode ser entregue mais de uma vez. Retries, redelivery e falhas durante o processamento fazem parte do funcionamento de sistemas distribuídos e tornam inadequada a suposição de que cada operação será executada exatamente uma vez.

Além disso, duplicidade de mensagem e duplicidade de negócio não são necessariamente a mesma coisa.

Duas mensagens diferentes podem representar o mesmo efeito de negócio:

```text
EventId A ──┐
            ├── OrderId X → registrar Payment
EventId B ──┘
```
Nesse cenário, proteger apenas o `EventId` não é suficiente. Como os eventos possuem identificadores diferentes, ambos podem ser considerados mensagens válidas e seguir para processamento.

Entretanto, do ponto de vista do negócio, os dois eventos representam a tentativa de registrar um pagamento para o mesmo pedido.

Sem idempotência de negócio, retries, redeliveries, eventos duplicados ou processamento concorrente poderiam resultar em múltiplos efeitos para uma operação que deveria ocorrer apenas uma vez.

Portanto, a necessidade de idempotência no OrderFlow não está apenas em evitar que uma mensagem seja processada novamente, mas principalmente em garantir que múltiplas tentativas não produzam efeitos de negócio duplicados.

## Decisão

O OrderFlow adotará idempotência tanto no nível da mensagem quanto no nível da operação de negócio.

A decisão parte do princípio de que esses dois níveis resolvem problemas diferentes:

```text
EventId
   ↓
identidade da mensagem
   ↓
Inbox

OrderId
   ↓
identidade do efeito de negócio
   ↓
Payment
```

Para o processamento de pagamentos, o `OrderId` foi escolhido como chave de idempotência de negócio porque representa o pedido para o qual o pagamento está sendo registrado.

A regra estabelecida é:

> Um pedido deve produzir no máximo um `Payment`.

Essa regra não dependerá apenas de uma consulta prévia na Application. Como múltiplos Workers podem executar a mesma verificação simultaneamente, a unicidade também será garantida pelo banco de dados.

Dessa forma, a solução combina proteção lógica e estrutural:

```text
Application
    ↓
verifica se o efeito já existe

Banco de dados
    ↓
garante UNIQUE(OrderId)
```

A Application evita operações desnecessárias no cenário comum, enquanto o banco fornece a garantia definitiva diante de concorrência.

Quando a restrição de unicidade detectar uma tentativa concorrente, o sistema verificará se o efeito esperado já foi produzido. Se o `Payment` para aquele `OrderId` existir, a operação será considerada concluída de forma idempotente.

## Consequências

A adoção de idempotência permite que o OrderFlow processe mensagens repetidas ou operações concorrentes sem produzir efeitos de negócio duplicados.

Para pagamentos, o resultado esperado permanece o mesmo independentemente do número de tentativas relacionadas ao mesmo pedido:

```text
1 tentativa  ─┐
2 tentativas ─┼── OrderId X → 1 Payment
N tentativas ─┘
```

A solução também estabelece uma separação importante entre confiabilidade da mensageria e consistência do negócio.

O Inbox responde à pergunta:

> Esta mensagem já foi processada?

A idempotência por `OrderId` responde:

> Este efeito de negócio já foi realizado?

Essa separação permite que cada mecanismo utilize a identidade adequada ao problema que precisa resolver.

Como consequência, o sistema passa a depender de uma restrição de unicidade no banco de dados para garantir a regra durante condições de corrida. Isso significa que violações de unicidade podem ocorrer como parte normal de um processamento concorrente e precisam ser interpretadas corretamente pela aplicação.

Há também um aumento de complexidade no fluxo de persistência, pois uma tentativa rejeitada pelo banco precisa ter seu estado de tracking tratado antes da continuidade do processamento.

Em contrapartida, o sistema obtém uma propriedade essencial para processamento distribuído:

> Repetir uma tentativa não deve significar repetir o efeito de negócio.