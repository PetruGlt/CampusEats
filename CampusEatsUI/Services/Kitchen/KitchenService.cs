using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Kitchen;

public class KitchenService(HttpClient _http) : IKitchenService
{
    public Task<List<Models.Orders>> GetPendingOrdersAsync()
    {
        throw new NotImplementedException();
    }

    public void UpdateOrderStatus(Guid id, string status)
    {
        throw new NotImplementedException();
    }

    public Task<KitchenDashboard> GetKitchenDashboardAsync()
    {
        throw new NotImplementedException();
    }

    public void BulkUpdateOrderStatusAsync(List<Guid> orderIds, string status)
    {
        throw new NotImplementedException();
    }

    public Task<List<PopularItem>> GetPopularItemsAsync(int topN)
    {
        throw new NotImplementedException();
    }
}