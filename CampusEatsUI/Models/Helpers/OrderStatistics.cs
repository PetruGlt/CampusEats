namespace CampusEatsUI.Models.Helpers;

public record OrderStatistics(
    int TotalOrders,
    int TotalRevenue,
    decimal AverageOrderValue,
    OrdersByStatus OrdersByStatus,
    int TodayOrders,
    int TodayRevenue
    );
    
public record OrdersByStatus( 
    int Cancelled,
    int Completed,
    int Ready,
    int Pending,
    int Preparing
    );