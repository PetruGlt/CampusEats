using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Persistence;
using CampusEats.Features.Orders;

namespace CampusEats.Features.Kitchen;

public record ViewPendingOrdersQuery : IRequest<IResult>;



public class ViewPendingOrdersHandler : IRequestHandler<ViewPendingOrdersQuery, IResult>
{
    private readonly CampusEatsContext _db;

    public ViewPendingOrdersHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(ViewPendingOrdersQuery request, CancellationToken ct)
    {
        var pendingOrders = await _db.Orders
            .Where(o => o.Status == "Pending" || o.Status == "Preparing")
            .ToListAsync(ct);

        return Results.Ok(pendingOrders);
    }
}
