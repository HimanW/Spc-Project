using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spc.Client.Models;
using Spc.Client.Services;
using Spc.Client.Utils;

namespace Spc.Client.Pages.Drugs;

public class IndexModel : PageModel
{
    private readonly SpcApiClient _api;
    public IndexModel(SpcApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    public PagedResult<DrugDto>? Result { get; set; }
    public string? Message { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Result = await _api.SearchDrugsAsync(Query, null, 1, 20, ct);
    }

    public async Task<IActionResult> OnPostAsync(Guid drugId, string drugName, decimal unitPrice, int qty)
    {
        var cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        var existing = cart.FirstOrDefault(x => x.DrugId == drugId);
        
        if (existing != null) existing.Quantity += qty;
        else cart.Add(new CartItem { DrugId = drugId, DrugName = drugName, UnitPrice = unitPrice, Quantity = qty });

        HttpContext.Session.SetJson("Cart", cart);
        Message = $"Added {drugName} to cart.";
        
        // Correct way: Just await the method, then return the page
        await OnGetAsync(CancellationToken.None);
        return Page();
    }
}