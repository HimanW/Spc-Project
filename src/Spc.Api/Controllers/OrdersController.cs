using Microsoft.AspNetCore.Mvc;
using Spc.Application.Orders;
namespace Spc.Api.Controllers;
[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase {
    private readonly IOrderService _orders;
    public OrdersController(IOrderService orders) => _orders = orders;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct) {
        var created = await _orders.CreateAsync(request, createdBy: "system", ct);
        return CreatedAtAction(nameof(GetById), new { orderId = created.OrderId }, created);
    }
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken ct) {
        var order = await _orders.GetByIdAsync(orderId, ct);
        return order is null ? NotFound() : Ok(order);
    }
    [HttpPatch("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct) {
        await _orders.UpdateStatusAsync(orderId, request, updatedBy: "system", ct);
        return Ok(new { message = "Order status updated." });
    }
}