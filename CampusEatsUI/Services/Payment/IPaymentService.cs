using CampusEatsUI.Models;

namespace CampusEatsUI.Services.Payment;

public interface IPaymentService
{
    public void CreateCheckoutSessionAsync(Guid id, Guid userId, string successUrl, string cancelUrl);
    public Task<List<Payments>> GetPaymentHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, string status);
    public Task<Payments> GetPaymentByIdAsync(Guid id);
}