using CampusEatsUI.Models;

namespace CampusEatsUI.Services.Payment;

public class PaymentService(HttpClient _http) : IPaymentService
{
    public void CreateCheckoutSessionAsync(Guid id, Guid userId, string successUrl, string cancelUrl)
    {
        throw new NotImplementedException();
    }

    public Task<List<Payments>> GetPaymentHistoryAsync(Guid userId, DateTime startDate, DateTime endDate, string status)
    {
        throw new NotImplementedException();
    }

    public Task<Payments> GetPaymentByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}