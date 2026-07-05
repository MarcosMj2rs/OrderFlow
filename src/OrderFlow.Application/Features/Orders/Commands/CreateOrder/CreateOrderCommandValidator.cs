using FluentValidation;

namespace OrderFlow.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();

            RuleFor(x => x.Items).NotEmpty();

            RuleForEach(x => x.Items).SetValidator(new CreateOrderItemCommandValidator());
        }
    }
}
