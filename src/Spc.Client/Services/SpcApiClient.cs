using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Spc.Client.Services;

public sealed class SpcApiClient
{
    private readonly HttpClient _http;
    private readonly ApiOptions _opt;

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
            var res = await _http.GetFromJsonAsync<PagedResult<DrugDto>>(url, ct);
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
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PharmacyDto>(cancellationToken: ct))!;
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
        // Status 5 = CANCELLED
        var resp = await _http.PatchAsJsonAsync(Url($"/api/v1/orders/{orderId}/status"),
            new UpdateOrderStatusRequest { Status = 5 }, ct);
        resp.EnsureSuccessStatusCode();
    }
}