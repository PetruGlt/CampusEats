namespace CampusEatsUI.Models.Requests.Kitchen;

public record UpdateOrderStatusRequest(Guid Id, string Status);