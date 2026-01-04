using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Kitchen;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using OrderModel = CampusEatsUI.Models.Orders;
using Orders = CampusEatsUI.Pages.Orders;

namespace CampusEatsUI.UI;

public class KitchenUiTests : BunitContext
{
    private readonly Mock<IKitchenService> _kitchenServiceMock;

    public KitchenUiTests()
    {
        _kitchenServiceMock = new Mock<IKitchenService>();
        Services.AddSingleton(_kitchenServiceMock.Object);
    }

    [Fact]
    public void Renders_Dashboard_And_Orders_Correctly()
    {
        // Arrange
        var orderItemId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var items = new List<OrderItems> 
        { 
            new OrderItems(orderItemId, orderId, menuItemId, "Burger",  10.00m, 2, "Yes")  
        };

        var pendingOrders = new List<OrderModel>
        {
            new Models.Orders(orderId, Guid.NewGuid(), items, "Pending", 20.00m, DateTime.Now, null, "Table 1")
        };
        var dashboard = new KitchenDashboard(5, 3, 2, orderId, DateTime.Now, 10.00m, DateTime.Now);

        _kitchenServiceMock.Setup(x => x.GetKitchenDashboardAsync()).ReturnsAsync(dashboard);
        _kitchenServiceMock.Setup(x => x.GetPendingOrdersAsync()).ReturnsAsync(pendingOrders);

        // Act
        var cut = Render<Kitchen>();

        // Assert
        Assert.Contains("5", cut.Find(".bg-warning .display-4").TextContent);
        Assert.Contains("3", cut.Find(".bg-info .display-4").TextContent);
        
        Assert.Contains(orderId.ToString().Substring(0, 6), cut.Markup);
        Assert.Contains("Burger", cut.Markup);
        Assert.Contains("2 x", cut.Markup);
    }

    [Fact]
    public void UpdateStatus_Calls_Service_And_Reloads()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var pendingOrders = new List<Models.Orders>
        {
            new Models.Orders(orderId, Guid.NewGuid(), new List<OrderItems>(), "Pending", 20.00m, DateTime.Now, null, "")
        };
        
        _kitchenServiceMock.Setup(x => x.GetKitchenDashboardAsync()).ReturnsAsync(new KitchenDashboard(0, 0, 0, Guid.Empty, DateTime.Now, 0, DateTime.Now));
        _kitchenServiceMock.Setup(x => x.GetPendingOrdersAsync()).ReturnsAsync(pendingOrders);
        
        var cut = Render<Kitchen>();

        // Act
        // "Mark Preparing" is the first button (btn-outline-primary)
        cut.Find("button.btn-outline-primary").Click();

        // Assert
        _kitchenServiceMock.Verify(x => x.UpdateOrderStatusAsync(orderId, "Preparing"), Times.Once);
        _kitchenServiceMock.Verify(x => x.GetPendingOrdersAsync(), Times.AtLeast(2));
    }
    
    [Fact]
    public void Refresh_Button_Reloads_Data()
    {
        // Arrange
        _kitchenServiceMock.Setup(x => x.GetKitchenDashboardAsync()).ReturnsAsync(new KitchenDashboard(0, 0, 0, Guid.Empty, DateTime.Now, 0, DateTime.Now));
        _kitchenServiceMock.Setup(x => x.GetPendingOrdersAsync()).ReturnsAsync(new List<Models.Orders>());
        
        var cut = Render<Kitchen>();

        // Act
        cut.Find("button.btn-primary").Click(); // Refresh button

        // Assert
        _kitchenServiceMock.Verify(x => x.GetPendingOrdersAsync(), Times.AtLeast(2));
    }
}