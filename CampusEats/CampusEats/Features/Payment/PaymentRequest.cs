namespace CampusEats.Features.Payment
{
    public class PaymentRequest
    {
        public long Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string ProductName { get; set; } = "Default product";
        public string SuccessUrl { get; set; } = "https://example.com/success";
        public string CancelUrl { get; set; } = "https://example.com/cancel";
    }
}