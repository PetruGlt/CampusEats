using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Orders;

public interface IOrderService
{
    public Task<Models.Orders> CreateOrderAsync(Guid userId, List<OrderItem> orderedItems, string? notes);
    public Task<List<Models.Orders>> GetAllOrdersAsync(Guid id);
    public Task<Models.Orders> GetOrderByIdAsync(Guid id);
    public Task CancelOrderAsync(Guid id);
    public Task<List<Models.Orders>> GetOrderHistoryAsync(DateTime startDate, DateTime endDate, Guid id);
    public Task<OrderStatistics> GetOrderStatisticsAsync();
    public Task<List<Models.Orders>> SearchOrdersAsync(string query, string status);
    public Task<OrderWaitTime> GetOrderWaitTimeAsync(Guid id);
}