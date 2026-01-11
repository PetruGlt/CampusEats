using System.Net.Http.Json;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Models.Requests.Kitchen;

namespace CampusEatsUI.Services.Kitchen;

public class KitchenService(HttpClient _http) : IKitchenService
{
    public async Task<List<Models.Orders>> GetPendingOrdersAsync()
    {
        return await _http.GetFromJsonAsync<List<Models.Orders>>("/api/kitchen/orders") ?? [];
    }

    public async Task UpdateOrderStatusAsync(Guid id, string status)
    {
        var request = new UpdateOrderStatusRequest(id, status);
        await _http.PutAsJsonAsync($"/api/kitchen/orders/{id}/status", request);
    }

    public async Task<KitchenDashboard> GetKitchenDashboardAsync()
    {
        return await _http.GetFromJsonAsync<KitchenDashboard>("/api/kitchen/dashboard") ?? new KitchenDashboard(0, 0, 0, Guid.Empty, DateTime.Now, decimal.Zero , DateTime.Now);
    }

    public async Task BulkUpdateOrderStatusAsync(List<Guid> orderIds, string status)
    {
        var request = new BulkUpdateOrderStatusRequest(orderIds, status);
        await _http.PutAsJsonAsync("/api/kitchen/orders/status", request);
    }

    public async Task<List<PopularItem>> GetPopularItemsAsync(int topN)
    {
        var request = new GetPopularItemsRequest(topN);
        return await _http.GetFromJsonAsync<List<PopularItem>>("/api/kitchen/popular-items?topN={topN}") ?? [];
    }
}