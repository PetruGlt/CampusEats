using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Kitchen;

public interface IKitchenService
{
    public Task<List<Models.Orders>> GetPendingOrdersAsync();
    public Task UpdateOrderStatusAsync(Guid id, string status);
    public Task<KitchenDashboard> GetKitchenDashboardAsync();
    public Task BulkUpdateOrderStatusAsync(List<Guid> orderIds, string status);
    public Task<List<PopularItem>> GetPopularItemsAsync(int topN);
}