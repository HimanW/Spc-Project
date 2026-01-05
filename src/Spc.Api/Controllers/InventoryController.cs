using Microsoft.AspNetCore.Mvc;
using Spc.Application.Inventory;
namespace Spc.Api.Controllers;
[ApiController]
[Route("api/v1/inventory")]
public class InventoryController : ControllerBase {
    private readonly IInventoryService _inventory;
    public InventoryController(IInventoryService inventory) => _inventory = inventory;

    [HttpPost("updates")]
    public async Task<IActionResult> ApplyUpdates([FromBody] InventoryUpdateBatchRequest request, CancellationToken ct) {
        await _inventory.ApplyUpdatesAsync(request, createdBy: "system", ct);
        return Ok(new { message = "Inventory updated." });
    }
    [HttpGet]
    public async Task<IActionResult> GetBalances([FromQuery] Guid? locationId, [FromQuery] Guid? drugId, CancellationToken ct) {
        return Ok(await _inventory.GetBalancesAsync(locationId, drugId, ct));
    }
}