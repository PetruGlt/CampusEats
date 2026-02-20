using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Orders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using OrderModel = CampusEatsUI.Models.Orders;

namespace CampusEatsUI.UI;

public class OrderDetailsUiTests : BunitContext
{
    private readonly Mock<IOrderService> _orderServiceMock;

    public OrderDetailsUiTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        Services.AddSingleton(_orderServiceMock.Object);
    }

    [Fact]
    public void Renders_Loading_When_Order_Is_Null()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(orderId))
            .ReturnsAsync((OrderModel?)null);

        // Act
        var cut = Render<OrderDetails>(parameters => parameters
            .Add(p => p.OrderId, orderId));

        // Assert
        Assert.Contains("Loading...", cut.Markup);
    }

    [Fact]
    public void Renders_Order_Details_When_Loaded()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        // Using empty list for items as constructor signature is unknown from context
        var order = new OrderModel(orderId, userId, new List<OrderItems>(), "Pending", 99.99m, DateTime.Now, null, "123 Test St");

        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var cut = Render<OrderDetails>(parameters => parameters
            .Add(p => p.OrderId, orderId));

        // Assert
        Assert.Contains(orderId.ToString().Substring(0, 8), cut.Markup);
        Assert.Contains("Pending", cut.Markup);
        Assert.Contains("$99.99", cut.Markup);
    }

    [Fact]
    public void Proceed_Button_Navigates_To_Orders()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderModel(orderId, Guid.NewGuid(), new List<OrderItems>(), "Pending", 10m, DateTime.Now, null, "");
        
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
        var navMan = Services.GetRequiredService<NavigationManager>();

        var cut = Render<OrderDetails>(parameters => parameters
            .Add(p => p.OrderId, orderId));

        // Act
        cut.Find("button.btn-success").Click();

        // Assert
        Assert.Equal("http://localhost/orders", navMan.Uri);
    }

    [Fact]
    public void Processed_Order_Shows_Message_Instead_Of_Button()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderModel(orderId, Guid.NewGuid(), new List<OrderItems>(), "Completed", 10m, DateTime.Now, null, "");
        
        _orderServiceMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

        // Act
        var cut = Render<OrderDetails>(parameters => parameters
            .Add(p => p.OrderId, orderId));

        // Assert
        Assert.Contains("This order has been processed", cut.Markup);
        Assert.Empty(cut.FindAll("button.btn-success"));
    }
}