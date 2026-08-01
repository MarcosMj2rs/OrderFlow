using Asp.Versioning;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Contracts.Orders.Requests;
using OrderFlow.Api.Contracts.Orders.Responses;
using OrderFlow.Application.Features.Orders.Commands.PayOrder;
using OrderFlow.Application.Features.Orders.Queries.GetOrderById;
using OrderFlow.Application.Features.Orders.Queries.GetOrders;
using ApiCreateOrderResponse = OrderFlow.Api.Contracts.Orders.Responses.CreateOrderResponse;
using ApplicationCreateOrderCommand = OrderFlow.Application.Features.Orders.Commands.CreateOrder.CreateOrderCommand;
using ApplicationCreateOrderResponse = OrderFlow.Application.Features.Orders.Commands.CreateOrder.CreateOrderResponse;
using ApplicationGetOrdersResponse = OrderFlow.Application.Features.Orders.Queries.GetOrders.GetOrdersResponse;

namespace OrderFlow.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public OrdersController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiCreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiCreateOrderResponse>> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        ApplicationCreateOrderCommand command = _mapper.Map<ApplicationCreateOrderCommand>(request);

        ApplicationCreateOrderResponse result = await _sender.Send(command, cancellationToken);

        ApiCreateOrderResponse response = _mapper.Map<ApiCreateOrderResponse>(result);

        return CreatedAtAction(nameof(CreateAsync), new { Version = "1", id = response.OrderId }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);

        GetOrderByIdResponse? result = await _sender.Send(query, cancellationToken);

        if (result is null)
            return NotFound();

        OrderResponse response = _mapper.Map<OrderResponse>(result);

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<OrderSummaryResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ApplicationGetOrdersResponse> result = await _sender.Send(new GetOrdersQuery(), cancellationToken);
        IReadOnlyCollection<OrderSummaryResponse> response = _mapper.Map<IReadOnlyCollection<OrderSummaryResponse>>(result);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PayAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new PayOrderCommand(id);
        await _sender.Send(command, cancellationToken);

        return NoContent();
    }
}
