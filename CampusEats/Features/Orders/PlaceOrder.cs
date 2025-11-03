using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Features.Orders;



public record PlaceOrderCommand(List<OrderItem> Items, Guid CustomerId) : IRequest<IResult>;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator(CampusEatsContext db)
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).MustAsync(async (item, ct) =>
        {
            var menuItemExists = await db.MenuItems.AnyAsync(m => m.Id == item.MenuItemId, ct);
            return menuItemExists;
        }).WithMessage("One or more menu items do not exist.");
    }
}

public class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand, IResult>
{
    private readonly CampusEatsContext _db;

    public PlaceOrderHandler(CampusEatsContext db)
    {
        _db = db;
    }
    public async Task<IResult> Handle(PlaceOrderCommand request, CancellationToken ct)
    {

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderItems = request.Items
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        Console.WriteLine($"Order {order.Id} placed for customer {order.CustomerId}.");

        return await Task.FromResult(Results.Created($"/orders/{order.Id}", order));
    }
}
