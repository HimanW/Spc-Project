using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spc.Client.Services;

public sealed class SpcApiClient
{
    private readonly HttpClient _http;
    private readonly ApiOptions _opt;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };


    public SpcApiClient(HttpClient http, IOptions<ApiOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    private string Url(string path) => $"{_opt.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    // -------- DRUGS ----------
    public async Task<PagedResult<DrugDto>> SearchDrugsAsync(string? query, string? category, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(query)) qs.Add($"query={Uri.EscapeDataString(query)}");
        if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        qs.Add($"page={page}");
        qs.Add($"pageSize={pageSize}");

        var url = Url($"/api/v1/drugs?{string.Join("&", qs)}");
        // Handle potential null response or errors gracefully in production
        try 
        {
            var res = await _http.GetFromJsonAsync<PagedResult<DrugDto>>(url, JsonOptions, ct);
            return res ?? new PagedResult<DrugDto> { Items = new List<DrugDto>(), Page = page, PageSize = pageSize, Total = 0 };
        }
        catch
        {
            return new PagedResult<DrugDto> { Items = new List<DrugDto>(), Page = page, PageSize = pageSize, Total = 0 };
        }
    }

    // -------- PHARMACY ----------
    public async Task<PharmacyDto> CreatePharmacyAsync(CreatePharmacyRequest req, CancellationToken ct)
{
    var resp = await _http.PostAsJsonAsync(Url("/api/v1/pharmacies"), req, ct);

    if (resp.IsSuccessStatusCode)
    {
        return (await resp.Content.ReadFromJsonAsync<PharmacyDto>(JsonOptions, ct))!;
    }

    // Read error message returned by your API middleware:
    // { error: "...", status: 409, path: "/api/v1/pharmacies" }
    ApiError? apiErr = null;
    try
    {
        apiErr = await resp.Content.ReadFromJsonAsync<ApiError>(cancellationToken: ct);
    }
    catch { /* ignore parse errors */ }

    var msg = apiErr?.Error ?? $"API Error: {(int)resp.StatusCode} {resp.ReasonPhrase}";
    throw new ApiException((int)resp.StatusCode, msg);
}


    // -------- ORDERS ----------
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync(Url("/api/v1/orders"), req, ct);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<OrderDto>(cancellationToken: ct))!;
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        try {
            return await _http.GetFromJsonAsync<OrderDto>(Url($"/api/v1/orders/{orderId}"), ct);
        } catch { return null; }
    }

    public async Task CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        var resp = await _http.PatchAsJsonAsync(
            Url($"/api/v1/orders/{orderId}/status"),
            new UpdateOrderStatusRequest { Status = "CANCELLED" }, // ✅ string
            ct);

        resp.EnsureSuccessStatusCode();
    }

}