using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Payment;

public class GetPaymentHistoryHandler(CampusEatsContext context)
{
    public async Task<List<PaymentHistoryResponse>> Handle(GetPaymentHistoryRequest request)
    {
        var query = context.Payments.AsQueryable();

        // Filter by user
        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(p => p.UserId == request.UserId);
        }

        // Filter by date range
        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.EndDate.Value);
        }

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(p => new PaymentHistoryResponse(
            p.Id,
            p.OrderId,
            p.Amount,
            p.Currency,
            p.Status.ToString(),
            p.UserId,
            p.CreatedAt,
            p.CompletedAt,
            p.FailureReason,
            p.ReceiptUrl
        )).ToList();
    }
}

