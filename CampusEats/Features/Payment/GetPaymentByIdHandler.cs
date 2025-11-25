using CampusEats.Exceptions;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Payment;

public class GetPaymentByIdHandler(CampusEatsContext context)
{
    public async Task<PaymentHistoryResponse> Handle(GetPaymentByIdRequest request)
    {
        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

        if (payment == null)
            throw new ValidationException($"Payment with ID {request.PaymentId} not found");

        return new PaymentHistoryResponse(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Status.ToString(),
            payment.UserId,
            payment.CreatedAt,
            payment.CompletedAt,
            payment.FailureReason,
            payment.ReceiptUrl
        );
    }
}


