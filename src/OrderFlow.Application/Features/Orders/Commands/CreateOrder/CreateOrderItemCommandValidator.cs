using FluentValidation;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
    {
        public CreateOrderItemCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();

            RuleFor(x => x.Quantity).GreaterThan(0);

            RuleFor(x => x.UnitPrice).GreaterThan(0);
        }
    }
}
