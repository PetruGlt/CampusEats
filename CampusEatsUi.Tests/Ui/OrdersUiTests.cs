using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using CampusEatsUI.Services.Orders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using OrderModel = CampusEatsUI.Models.Orders;

namespace CampusEatsUI.UI;

public class OrdersUiTests : BunitContext
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public OrdersUiTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _authServiceMock = new Mock<IAuthenticationService>();

        Services.AddSingleton(_orderServiceMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
    }

    [Fact]
    public void OnInitialized_Redirects_To_Login_If_No_Cookie()
    {
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var navMan = Services.GetRequiredService<NavigationManager>();

        Render<Pages.Orders>();

        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void Renders_Orders_List_Correctly()
    {
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        
        var orderId = Guid.NewGuid();
        var orders = new List<OrderModel>
        {
            new OrderModel(orderId, userId, new List<OrderItems>(), "Pending", 25.50m, DateTime.Now, null, "Address")
        };

        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(userId)).ReturnsAsync(orders);

        var cut = Render<Pages.Orders>();

        Assert.Contains(orderId.ToString().Substring(0, 8), cut.Markup);
        Assert.Contains("$25.50", cut.Markup);
        Assert.Contains("Pending", cut.Markup);
    }

    [Fact]
    public void Search_Filters_Orders()
    {
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        
        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(userId)).ReturnsAsync(new List<OrderModel>());
        
        var cut = Render<Pages.Orders>();
        
        var searchQuery = "test search";
        _orderServiceMock.Setup(x => x.SearchOrdersAsync(searchQuery, "")).ReturnsAsync(new List<OrderModel>());

        cut.Find("input").Change(searchQuery);
        cut.Find("button.btn-outline-secondary").Click(); // Search button

        _orderServiceMock.Verify(x => x.SearchOrdersAsync(searchQuery, ""), Times.Once);
    }

    [Fact]
    public void Cancel_Order_Calls_Service_And_Reloads()
    {
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        var orderId = Guid.NewGuid();
        var orders = new List<OrderModel>
        {
            new OrderModel(orderId, userId, new List<OrderItems>(), "Pending", 10m, DateTime.Now, null, "")
        };

        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(userId)).ReturnsAsync(orders);
        _orderServiceMock.Setup(x => x.CancelOrderAsync(orderId)).Returns(Task.CompletedTask);

        var cut = Render<Pages.Orders>();

        cut.Find("button.btn-warning").Click(); // Cancel button

        _orderServiceMock.Verify(x => x.CancelOrderAsync(orderId), Times.Once);
        _orderServiceMock.Verify(x => x.GetAllOrdersAsync(userId), Times.AtLeast(2)); // Initial + Reload
    }

    [Fact]
    public void CheckWaitTime_Displays_Alert()
    {
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        var orderId = Guid.NewGuid();
        var orders = new List<OrderModel>
        {
            new OrderModel(orderId, userId, new List<OrderItems>(), "Cooking", 10m, DateTime.Now, null, "")
        };

        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(userId)).ReturnsAsync(orders);
        
        _orderServiceMock.Setup(x => x.GetOrderWaitTimeAsync(orderId))
            .ReturnsAsync(new OrderWaitTime(orderId, "Cooking", 15, DateTime.Now, null));

        var cut = Render<Pages.Orders>();

        cut.Find("button.btn-success").Click(); // Wait Time button

        cut.WaitForState(() => cut.FindAll(".alert-warning").Count > 0);
        Assert.Contains("15 minutes", cut.Find(".alert-warning").TextContent);
    }

    [Fact]
    public void ViewDetails_Navigates_To_Details_Page()
    {
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        var orderId = Guid.NewGuid();
        var orders = new List<OrderModel>
        {
            new OrderModel(orderId, userId, new List<OrderItems>(), "Pending", 10m, DateTime.Now, null, "")
        };

        _orderServiceMock.Setup(x => x.GetAllOrdersAsync(userId)).ReturnsAsync(orders);
        var navMan = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Pages.Orders>();

        cut.Find("button.btn-secondary").Click(); // Details button

        Assert.Equal($"http://localhost/orders/{orderId}", navMan.Uri);
    }

    private void SetupAuth(Guid? userId = null)
    {
        var uid = userId ?? Guid.NewGuid();
        JSInterop.Setup<string>("eval", "document.cookie").SetResult("auth=token");
        _authServiceMock.Setup(x => x.GetTokenAsync(It.IsAny<string>())).Returns("token");
        _authServiceMock.Setup(x => x.ParseJwt("token")).Returns(new UserSession { Id = uid, Username = "Test User", Email = "test@example.com" });
    }
}