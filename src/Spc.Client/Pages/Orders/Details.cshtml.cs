using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spc.Client.Services;

namespace Spc.Client.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly SpcApiClient _api;
    public DetailsModel(SpcApiClient api) => _api = api;

    [BindProperty(SupportsGet = true)] public Guid OrderId { get; set; }
    public OrderDto? Order { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => Order = await _api.GetOrderAsync(OrderId, ct);

    public async Task<IActionResult> OnPostAsync(Guid orderId, CancellationToken ct)
    {
        await _api.CancelOrderAsync(orderId, ct);
        return RedirectToPage(new { OrderId = orderId });
    }
}