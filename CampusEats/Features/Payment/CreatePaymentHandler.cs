using CampusEats.Exceptions;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace CampusEats.Features.Payment;

public class CreatePaymentHandler(CampusEatsContext context)
{
    public async Task<PaymentResponse> Handle(CreatePaymentRequest request)
    {
        // Get the order
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            throw new OrderNotFoundException(request.OrderId);

        // Check if payment already exists for this order
        var existingPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && 
                                     p.Status != PaymentStatus.Failed && 
                                     p.Status != PaymentStatus.Cancelled);
        
        if (existingPayment != null)
            throw new ValidationException("Payment already exists for this order");

        if (!order.Items.Any())
            throw new ValidationException("Order has no items");

        var invalidItems = order.Items.Where(i => i.Price <= 0 || i.Quantity <= 0).ToList();
        if (invalidItems.Any())
        {
            var itemNames = string.Join(", ", invalidItems.Select(i => i.MenuItemName));
            throw new ValidationException($"Order contains items with invalid price or quantity: {itemNames}. All items must have price > 0 and quantity > 0.");
        }

        if (order.TotalAmount <= 0)
            throw new ValidationException($"Order total amount must be greater than 0. Current amount: {order.TotalAmount}");

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = order.Items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = Math.Max(1, (long)(item.Price * 100)), // Convert to cents, minimum 1 cent
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.MenuItemName,
                        Description = item.SpecialInstructions
                    }
                },
                Quantity = item.Quantity
            }).ToList(),
            Mode = "payment",
            SuccessUrl = string.IsNullOrEmpty(request.SuccessUrl) ? "http://localhost:5000/payment_success" : request.SuccessUrl,
            CancelUrl = string.IsNullOrEmpty(request.CancelUrl) ? "http://localhost:5000/payment_failure" : request.CancelUrl,
            ClientReferenceId = request.OrderId.ToString(),
            Metadata = new Dictionary<string, string>
            {
                { "order_id", request.OrderId.ToString()},
                { "user_id", request.UserId.ToString()}
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        if (string.IsNullOrEmpty(session.Url))
        {
            throw new ValidationException("Failed to create Stripe checkout session: URL is null");
        }

        var payment = new global::Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            StripeSessionId = session.Id,
            StripePaymentIntentId = session.PaymentIntentId ?? string.Empty,
            Amount = (long)(order.TotalAmount * 100), // Store in cents
            Currency = "usd",
            Status = PaymentStatus.Pending,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync();

        return new PaymentResponse(
            payment.Id,
            session.Id,
            session.Url ?? string.Empty,
            payment.Status.ToString(),
            payment.Amount,
            payment.Currency
        );
    }
}
