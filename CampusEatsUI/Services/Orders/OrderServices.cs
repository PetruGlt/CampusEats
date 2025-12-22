using System.Net.Http.Json;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Models.Requests.Orders;

namespace CampusEatsUI.Services.Orders;

public class OrderServices(HttpClient _http) : IOrderService
{
    public async void CreateOrderAsync(Guid userId, List<OrderItem> orderedItems, string? notes)
    {
        var request = new CreateOrderRequest(userId, orderedItems, notes);
        
        var response = await _http.PostAsJsonAsync("http://localhost:5168/orders", request);
        if (response.IsSuccessStatusCode)
        {
            var createdOrder = await response.Content.ReadFromJsonAsync<Models.Orders>();
            
        }
    }

    public Task<List<Models.Orders>> GetAllOrdersAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Models.Orders> GetOrderByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public void CancelOrderAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Models.Orders>> GetOrderHistoryAsync(DateTime startDate, DateTime endDate, Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<OrderStatistics> GetOrderStatisticsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<Models.Orders>> SearchOrdersAsync(string query, string status)
    {
        throw new NotImplementedException();
    }

    public Task<OrderWaitTime> GetOrderWaitTimeAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}