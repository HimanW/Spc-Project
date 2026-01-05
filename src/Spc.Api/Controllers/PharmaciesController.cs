using Microsoft.AspNetCore.Mvc;
using Spc.Application.Pharmacies;
namespace Spc.Api.Controllers;
[ApiController]
[Route("api/v1/pharmacies")]
public class PharmaciesController : ControllerBase {
    private readonly IPharmacyService _pharmacies;
    public PharmaciesController(IPharmacyService pharmacies) => _pharmacies = pharmacies;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePharmacyRequest request, CancellationToken ct) {
        var created = await _pharmacies.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { pharmacyId = created.PharmacyId }, created);
    }
    [HttpGet("{pharmacyId:guid}")]
    public async Task<IActionResult> GetById(Guid pharmacyId, CancellationToken ct) {
        var ph = await _pharmacies.GetByIdAsync(pharmacyId, ct);
        return ph is null ? NotFound() : Ok(ph);
    }
}