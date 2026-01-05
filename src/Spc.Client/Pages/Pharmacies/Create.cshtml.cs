using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spc.Client.Services;

namespace Spc.Client.Pages.Pharmacies;

public class CreateModel : PageModel
{
    private readonly SpcApiClient _api;
    public CreateModel(SpcApiClient api) => _api = api;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string RegNo { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";

    public string? CreatedMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        // Type 2 = Dealer
        var req = new CreatePharmacyRequest { Name = Name, Type = 2, RegNo = RegNo, Address = Address };
        var created = await _api.CreatePharmacyAsync(req, ct);

        HttpContext.Session.SetString("PharmacyId", created.PharmacyId.ToString());
        CreatedMessage = $"Success! Pharmacy registered. ID: {created.PharmacyId}";
        return Page();
    }
}