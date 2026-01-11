using System.Net.Http.Json;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Models.Requests.Orders;

namespace CampusEatsUI.Services.Orders;

public class OrderServices(HttpClient _http) : IOrderService
{
    
    public async Task<Models.Orders> CreateOrderAsync(Guid userId, List<OrderItem> orderedItems, string? notes)
    {
        var request = new CreateOrderRequest(userId, orderedItems, notes);
        var response = await _http.PostAsJsonAsync($"/api/orders", request);
        return await response.Content.ReadFromJsonAsync<Models.Orders>() ?? new Models.Orders(Guid.Empty, Guid.Empty, new List<Models.OrderItems>(), string.Empty, 0, DateTime.UtcNow, null, string.Empty);
    }

    public async Task<List<Models.Orders>> GetAllOrdersAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<List<Models.Orders>>($"/api/orders?userId={id}").ContinueWith(t => t.Result ?? []);
    }

    public async Task<Models.Orders> GetOrderByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Models.Orders>(($"/api/orders/{id}"))!;
    }

    public async Task CancelOrderAsync(Guid id)
    {
        var request = new CancellOrderRequest(id);
        await _http.PutAsJsonAsync($"/api/orders/{id}/cancel", request);
    }

    public async Task<List<Models.Orders>> GetOrderHistoryAsync(DateTime startDate, DateTime endDate, Guid id)
    {
        string fullUri = $"/api/orders/history?startDate={startDate:0}&endDate={endDate:0}&userId={id}";
        return await _http.GetFromJsonAsync<List<Models.Orders>>(fullUri).ContinueWith(t => t.Result ?? []);
    }

    public async Task<OrderStatistics> GetOrderStatisticsAsync()
    {
        return await _http.GetFromJsonAsync<OrderStatistics>($"/api/orders/statistics")!;
    }

    public async Task<List<Models.Orders>> SearchOrdersAsync(string query, string status)
    {
        return await _http.GetFromJsonAsync<List<Models.Orders>>($"/api/orders/search?query={query}&status={status}")
            .ContinueWith(t => t.Result ?? []);
    }

    public async Task<OrderWaitTime> GetOrderWaitTimeAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<OrderWaitTime>($"/api/orders/{id}/wait-time")!;
    }
}