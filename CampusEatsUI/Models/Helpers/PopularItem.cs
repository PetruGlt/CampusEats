namespace CampusEatsUI.Models.Helpers;

public record PopularItem(
    Guid MenuItemId,
    string MenuItemName,
    int TotalQuantitySold,    
    int TimesOrdered,
    decimal TotalRevenue
    );