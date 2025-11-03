using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;

// 1. Command
public record CancelOrderCommand(Guid OrderId) : IRequest<IResult>;

// 2. Validator
public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

// 3. Handler
public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, IResult>
{
    private readonly CampusEatsContext _db;

    public CancelOrderHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order == null)
        {
            return Results.NotFound($"Order with ID {request.OrderId} not found.");
        }

        if (order.Status != "Pending")
        {
            return Results.BadRequest($"Cannot cancel order. Status is '{order.Status}'.");
        }

        order.Status = "Cancelled";
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"Order {order.Id} has been cancelled.");

        return await Task.FromResult(Results.NoContent());
    }
}