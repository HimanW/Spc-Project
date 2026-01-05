using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spc.Client.Models;
using Spc.Client.Services;
using Spc.Client.Utils;

namespace Spc.Client.Pages.Cart;

public class IndexModel : PageModel
{
    private readonly SpcApiClient _api;
    public IndexModel(SpcApiClient api) => _api = api;

    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string? PharmacyId { get; set; }
    [BindProperty] public string? Notes { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }
    public Guid? CreatedOrderId { get; set; }

    public void OnGet()
    {
        PharmacyId = HttpContext.Session.GetString("PharmacyId");
        Items = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        Total = Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        OnGet(); // Load data
        if (string.IsNullOrEmpty(PharmacyId)) { Error = "Register a pharmacy first!"; return Page(); }
        if (!Items.Any()) { Error = "Cart is empty"; return Page(); }

        try {
            var req = new CreateOrderRequest {
                PharmacyId = Guid.Parse(PharmacyId),
                Notes = Notes,
                Items = Items.Select(x => new CreateOrderItemDto { DrugId = x.DrugId, Quantity = x.Quantity }).ToList()
            };
            var order = await _api.CreateOrderAsync(req, ct);
            Success = "Order Placed!";
            CreatedOrderId = order.OrderId;
            HttpContext.Session.Remove("Cart"); // Clear cart
            Items.Clear(); 
        } catch (Exception ex) { Error = ex.Message; }
        
        return Page();
    }
}