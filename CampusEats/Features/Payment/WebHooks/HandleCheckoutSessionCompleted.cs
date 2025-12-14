using CampusEats.Features.Loyalty;
using CampusEats.Features.Orders;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace CampusEats.Features.Webhooks;

public class HandleStripeWebhook(
    CampusEatsContext context,
    LoyaltyService loyaltyService,
    IConfiguration configuration,
    ILogger<HandleStripeWebhook> logger)
{
    public async Task<bool> Handle(string json, string signature)
    {
        try
        {
            var webhookSecret = configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.ClientReferenceId == null) return false;

                var orderId = Guid.Parse(session.ClientReferenceId);

                var payment = await context.Payments
                    .FirstOrDefaultAsync(p => p.StripeSessionId == session.Id);

                if (payment != null)
                {
                    payment.Status = PaymentStatus.Succeeded;
                    payment.StripePaymentIntentId = session.PaymentIntentId ?? payment.StripePaymentIntentId;
                    payment.CompletedAt = DateTime.UtcNow;
                }

                var order = await context.Orders.FindAsync(orderId);
                if (order != null && order.Status == OrderStatus.Pending)
                {
                    order.Status = OrderStatus.Preparing;
                    order.UpdatedAt = DateTime.UtcNow;

                    await loyaltyService.AddPointsForOrder(order.UserId, order.TotalAmount);
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Payment completed for order {OrderId}", orderId);
            }

            return true;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Webhook signature verification failed");
            return false;
        }
    }
}
