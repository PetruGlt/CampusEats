using CampusEats.Persistence;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Kitchen;

public record UpdateOrderStatusCommand(Guid OrderId, string NewStatus) : IRequest<IResult>;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.NewStatus).NotEmpty().Must(status =>
                new[] { "Preparing", "Ready", "Completed" }.Contains(status))
            .WithMessage("Status must be one of: Preparing, Ready, Completed.");
    }
}

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, IResult>
{
    private readonly CampusEatsContext _db;

    public UpdateOrderStatusHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(UpdateOrderStatusCommand request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order == null)
        {
            return Results.NotFound($"Order with ID {request.OrderId} not found.");
        }

        order.Status = request.NewStatus;
        await _db.SaveChangesAsync(ct);
        Console.WriteLine($"Order {order.Id} status updated to {order.Status}.");

        return Results.Ok(new { Message = "Order status updated successfully." });
    }
}

public record UpdateOrderStatusRequest(string NewStatus);
