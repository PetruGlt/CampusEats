using CampusEats.Persistence;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CampusEats.Features.Kitchen;

// 1. Query
public record GetDailyReportQuery : IRequest<IResult>;

// 2. Handler
public class GetDailyReportHandler : IRequestHandler<GetDailyReportQuery, IResult>
{
    private readonly CampusEatsContext _db;

    public GetDailyReportHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(GetDailyReportQuery request, CancellationToken ct)
    {
        var completedOrders = await _db.Orders
            .Where(o => o.Status == "Completed")
            .Include(o => o.OrderItems)
            .ToListAsync(ct);

        var report = new
        {
            TotalCompletedOrders = completedOrders.Count,
            TotalItemsSold = completedOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity),
            Orders = completedOrders
        };

        return await Task.FromResult(Results.Ok(report));
    }
}
