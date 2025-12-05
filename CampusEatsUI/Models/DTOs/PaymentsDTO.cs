namespace CampusEatsUI.Models.DTOs;

public record CreatePaymentRequest(Guid OrderId, decimal Amount, string Currency = "usd");
public record PaymentDto(Guid Id, Guid OrderId, decimal Amount, string Status, DateTime CreatedAt);
