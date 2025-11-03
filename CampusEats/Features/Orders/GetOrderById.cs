using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Persistence;

namespace CampusEats.Features.Orders;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<IResult>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, IResult>
{
    private readonly CampusEatsContext _db;

    public GetOrderByIdHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _db.Orders.Include(o => o.OrderItems)
                                    .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order == null)
        {
            return Results.NotFound($"Order with ID {request.OrderId} not found.");
        }

        return Results.Ok(order);
    }
}
