using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Payment;

public interface IPaymentService
{
    public Task CreateCheckoutSessionAsync(Guid orderId, Guid userId, string successUrl, string cancelUrl);
    public Task<List<PaymentHistoryResponse>> GetPaymentHistoryAsync(string userId, DateTime startDate, DateTime endDate, string status);
    public Task<Payments> GetPaymentByIdAsync(Guid id);
}