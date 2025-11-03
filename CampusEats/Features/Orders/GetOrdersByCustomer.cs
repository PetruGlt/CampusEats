using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders
{
    public record GetOrdersByCustomerQuery(Guid CustomerId) : IRequest<IResult>;

    public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, IResult>
    {
        private readonly CampusEatsContext _db;

        public GetOrdersByCustomerHandler(CampusEatsContext db)
        {
            _db = db;
        }

        public async Task<IResult> Handle(GetOrdersByCustomerQuery request, CancellationToken ct)
        {
            var orders = await _db.Orders
                .Where(o => o.CustomerId == request.CustomerId)
                .Include(o => o.OrderItems)
                .ToListAsync(ct);

            return Results.Ok(orders);
        }
    }

    public static class GetOrdersByCustomerEndpoint
    {
        public static void MapGetOrdersByCustomer(this IEndpointRouteBuilder app)
        {
            app.MapGet("/orders/customer/{customerId:guid}", async (Guid customerId, IMediator mediator) =>
                {
                    var query = new GetOrdersByCustomerQuery(customerId);
                    return await mediator.Send(query);
                })
                .WithName("GetOrdersByCustomer")
                .WithTags("Orders");
        }
    }
}
