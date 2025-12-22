namespace CampusEatsUI.Models.Requests.Kitchen;

public record BulkUpdateOrderStatusRequest(List<Guid> OrderIds, string Status);