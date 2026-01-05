using Microsoft.AspNetCore.Mvc;
using Spc.Application.Drugs;

namespace Spc.Api.Controllers;

[ApiController]
[Route("api/v1/drugs")]
public class DrugsController : ControllerBase
{
    private readonly IDrugService _drugService;

    public DrugsController(IDrugService drugService) => _drugService = drugService;

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _drugService.SearchAsync(query, category, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{drugId:guid}")]
    public async Task<IActionResult> GetById(Guid drugId, CancellationToken ct = default)
    {
        var drug = await _drugService.GetByIdAsync(drugId, ct);
        return drug is null ? NotFound() : Ok(drug);
    }
}