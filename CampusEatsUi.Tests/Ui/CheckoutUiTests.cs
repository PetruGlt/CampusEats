using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Helpers;
using CampusEatsUI.Services.Menu;
using CampusEatsUI.Services.Orders;
using CampusEatsUI.Services.Payment;
using CampusEatsUI.Services.UserLoyalty;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Orders = CampusEatsUI.Models.Orders;

namespace CampusEatsUI.UI;

public class CheckoutUiTests : BunitContext
{
    private readonly Mock<IMenuService> _menuServiceMock;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IUserLoyaltyService> _userLoyaltyServiceMock;
    private readonly CartState _cartState;

    public CheckoutUiTests()
    {
        _menuServiceMock = new Mock<IMenuService>();
        _orderServiceMock = new Mock<IOrderService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _userLoyaltyServiceMock = new Mock<IUserLoyaltyService>();
        _cartState = new CartState();

        Services.AddSingleton(_menuServiceMock.Object);
        Services.AddSingleton(_orderServiceMock.Object);
        Services.AddSingleton(_paymentServiceMock.Object);
        Services.AddSingleton(_userLoyaltyServiceMock.Object);
        Services.AddSingleton(_cartState);
    }

    [Fact]
    public void Renders_CartItems_And_Total_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _cartState.UserId = userId;
        _cartState.CartItems = new List<OrderItem>
        {
            new OrderItem(menuItemId, 2, "Test Item")
        };

        var menuItems = new List<MenuItem>
        {
            new MenuItem(menuItemId, "Pizza", 10.00m)
        };

        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);
        _userLoyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(new UserPoints(userId, 0, DateTime.Now)); // No points

        // Act
        var cut = Render<Checkout>();

        // Assert
        Assert.Contains("Pizza", cut.Markup);
        Assert.Contains("$10.00 x 2", cut.Markup);
        Assert.Contains("$20.00", cut.Markup); // Total
        Assert.DoesNotContain("Use Loyalty Points", cut.Markup);
    }

    [Fact]
    public void Renders_LoyaltyOption_When_Points_Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _cartState.UserId = userId;
        _cartState.CartItems = new List<OrderItem>
        {
            new OrderItem(menuItemId, 1, "Test Item")
        };

        var menuItems = new List<MenuItem>
        {
            new MenuItem(menuItemId, "Pizza", 10.00m)
        };

        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);
        _userLoyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(new UserPoints(userId, 100, DateTime.Now)); // Has points

        // Act
        var cut = Render<Checkout>();

        // Assert
        Assert.Contains("Use Loyalty Points", cut.Markup);
    }

    [Fact]
    public void Toggling_Loyalty_Updates_Total()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _cartState.UserId = userId;
        _cartState.CartItems = new List<OrderItem>
        {
            new OrderItem(menuItemId, 1, "Test Item") // Total $100
        };

        var menuItems = new List<MenuItem>
        {
            new MenuItem(menuItemId, "Expensive Pizza", 100.00m)
        };

        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);
        _userLoyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(new UserPoints(userId, 100, DateTime.Now));

        var cut = Render<Checkout>();

        // Act
        cut.Find("#useLoyaltyPoints").Change(true);

        // Assert
        // Total should be 75.00 (25% off 100)
        Assert.Contains("$75.00", cut.Markup);
    }

    [Fact]
    public void Pay_Button_CreatesOrder_And_Redirects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _cartState.UserId = userId;
        _cartState.CartItems = new List<OrderItem>
        {
            new OrderItem(menuItemId, 1, "Test Item")
        };

        var menuItems = new List<MenuItem> { new MenuItem(menuItemId, "Pizza", 10.00m) };
        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);
        _userLoyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId)).ReturnsAsync(new UserPoints(userId, 0, DateTime.Now));

        var orderId = Guid.NewGuid();
        _orderServiceMock.Setup(x => x.CreateOrderAsync(userId, _cartState.CartItems, It.IsAny<string>()))
            .ReturnsAsync(new Orders(orderId, userId, new List<OrderItems>(), string.Empty, 0, DateTime.Now, null, string.Empty));

        var navMan = Services.GetRequiredService<NavigationManager>();
        var cut = Render<Checkout>();

        // Act
        cut.Find("button.btn-success").Click();

        // Assert
        _orderServiceMock.Verify(x => x.CreateOrderAsync(userId, _cartState.CartItems, It.IsAny<string>()), Times.Once);
        _paymentServiceMock.Verify(x => x.CreateCheckoutSessionAsync(orderId, userId, null, null), Times.Once);
        Assert.Equal("http://localhost/orders", navMan.Uri);
    }
}