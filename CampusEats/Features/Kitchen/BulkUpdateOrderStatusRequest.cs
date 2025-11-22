namespace CampusEats.Features.Kitchen;

public record BulkUpdateOrderStatusRequest(List<Guid> OrderIds, string Status);
