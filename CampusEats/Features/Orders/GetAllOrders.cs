using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders
{
    public record GetAllOrdersQuery : IRequest<IResult>;

    public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, IResult>
    {
        private readonly CampusEatsContext _db;

        public GetAllOrdersHandler(CampusEatsContext db)
        {
            _db = db;
        }

        public async Task<IResult> Handle(GetAllOrdersQuery request, CancellationToken ct)
        {
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .ToListAsync(ct);

            return Results.Ok(orders);
        }
    }
    
    public static class GetAllOrdersEndpoint
    {
        public static void MapGetAllOrders(this IEndpointRouteBuilder app)
        {
            app.MapGet("/orders", async (IMediator mediator) =>
                {
                    var query = new GetAllOrdersQuery();
                    return await mediator.Send(query);
                })
                .WithName("GetAllOrders")
                .WithTags("Orders");
        }
    }
}
