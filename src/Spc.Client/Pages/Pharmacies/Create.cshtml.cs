using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spc.Client.Services;

namespace Spc.Client.Pages.Pharmacies;

public class CreateModel : PageModel
{
    private readonly SpcApiClient _api;

    public CreateModel(SpcApiClient api) => _api = api;

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Type { get; set; } = "SPC";   // ✅ string
    [BindProperty] public string RegNo { get; set; } = "";
    [BindProperty] public string Address { get; set; } = "";

    public string? CreatedMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        try
        {
            var created = await _api.CreatePharmacyAsync(new CreatePharmacyRequest
            {
                Name = Name,
                Type = Type,
                RegNo = RegNo,
                Address = Address
            }, ct);

            HttpContext.Session.SetString("PharmacyId", created.PharmacyId.ToString());
            CreatedMessage = $"Created pharmacy: {created.Name} (ID: {created.PharmacyId})";
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.StatusCode == 409
                ? $"Conflict: {ex.Message} (Try a different RegNo)"
                : $"API error ({ex.StatusCode}): {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unexpected error: {ex.Message}";
        }

        return Page();
    }
}
