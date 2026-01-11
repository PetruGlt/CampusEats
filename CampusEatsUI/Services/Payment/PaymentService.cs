using System.Net.Http.Json;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Models.Requests.Payment;

namespace CampusEatsUI.Services.Payment;

public class PaymentService(HttpClient _http) : IPaymentService
{
    public async Task<PaymentResponse> CreateCheckoutSessionAsync(Guid orderId, Guid userId, string successUrl, string cancelUrl)
    {
        var request = new CreateCheckoutSessionRequest(orderId, userId, successUrl, cancelUrl);
        var response = await _http.PostAsJsonAsync($"/api/payments/create-checkout", request);
        return await response.Content.ReadFromJsonAsync<PaymentResponse>();
    }

    public async Task<List<PaymentHistoryResponse>> GetPaymentHistoryAsync(string userId, DateTime startDate, DateTime endDate,
        string status)
    {
        var results = await _http.GetFromJsonAsync<List<PaymentHistoryResponse>>($"/api/payments/history?userId={userId}");
        return results;
    }

    public async Task<Payments> GetPaymentByIdAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Payments>($"/api/payments/{id}")!;
    }
}