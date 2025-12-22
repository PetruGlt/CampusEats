namespace CampusEatsUI.Models.Helpers;

public record KitchenDashboard(
    int PendingOrdersCount,
    int PreparingOrdersCount,
    int ReadyOrdersCount,
    Guid OldestPendingOrderId,
    DateTime OldestPreparationTime,
    decimal AveragePreparationTimeMinutes,
    DateTime EstimationCompletionTime);
    