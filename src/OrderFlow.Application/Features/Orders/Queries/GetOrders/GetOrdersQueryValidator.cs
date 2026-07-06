using FluentValidation;

namespace OrderFlow.Application.Features.Orders.Queries.GetOrders
{
    public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
    {
        public GetOrdersQueryValidator() { }
    }
}
