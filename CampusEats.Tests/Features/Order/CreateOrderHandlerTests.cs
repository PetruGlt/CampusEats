using CampusEats.Exceptions;
using CampusEats.Features.Menu;
using CampusEats.Features.Orders;
using CampusEats.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Features.Order;
public class CreateOrderHandlerTests : IDisposable
{
    [Fact]
    public async void GivenValidOrder_WhenCreateOrder_ThenCreatesOrderAndComputesTotal()
    {
        var context = ContextHelper.CreateInMemoryDBContext();
        
        var menuItem1 = new MenuItem(Guid.NewGuid(), "Burger", 10.00m);
        var menuItem2 = new MenuItem(Guid.NewGuid(), "Fries", 5.00m);
        context.Add(menuItem1);
        context.Add(menuItem2);
        
        await context.SaveChangesAsync();

        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest(
            UserId: "user123",
            Items: new List<OrderItemDto>
            {
                new (MenuItemId: menuItem1.Id, Quantity: 2, SpecialInstructions: "No onions"),
                new (MenuItemId: menuItem2.Id, Quantity: 1, SpecialInstructions: null)
            },
            Notes: "Extra napkins please"
            );

        var response = await handler.Handle(request);

        response.Should().NotBeNull();
        response.TotalAmount.Should().Be(25.00m); 
        response.Status.Should().Be("Pending");
        response.Items.Should().HaveCount(2);
        
        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == response.Id);
        order.Should().NotBeNull();
        order!.Items.Should().HaveCount(2);
        
    }
    
    [Fact]
    public async void GivenNonExistentMenuItem_WhenCreateOrder_ThenThrowsMenuItemNotFoundException()
    {
        var context = ContextHelper.CreateInMemoryDBContext();
        var handler = new CreateOrderHandler(context);
        var request = new CreateOrderRequest(
            UserId: "user123",
            Items: new List<OrderItemDto>
            {
                new (MenuItemId: Guid.NewGuid(), Quantity: 2, SpecialInstructions: null)
            },
            Notes: null
            );

        await Assert.ThrowsAsync<MenuItemsNotFoundException>(() => handler.Handle(request));
    }
    
    public void Dispose()
    {
        
    }
}