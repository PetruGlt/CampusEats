using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using CampusEatsUI.Services.Helpers;
using CampusEatsUI.Services.Menu;
using CampusEatsUI.Services.Orders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CampusEatsUI.UI;

public class MenuUiTests : BunitContext
{
    private readonly Mock<IMenuService> _menuServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly CartState _cartState;

    public MenuUiTests()
    {
        _menuServiceMock = new Mock<IMenuService>();
        _authServiceMock = new Mock<IAuthenticationService>();
        _orderServiceMock = new Mock<IOrderService>();
        _cartState = new CartState();

        Services.AddSingleton(_menuServiceMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
        Services.AddSingleton(_orderServiceMock.Object);
        Services.AddSingleton(_cartState);
    }

    [Fact]
    public void OnInitialized_Redirects_To_Login_If_No_Cookie()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<Menu>();

        // Assert
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void OnInitialized_Redirects_To_Login_If_Auth_Fails()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult("auth=token");
        _authServiceMock.Setup(x => x.GetTokenAsync(It.IsAny<string>())).Returns("token");
        _authServiceMock.Setup(x => x.ParseJwt("token")).Throws(new Exception("Invalid token"));
        
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<Menu>();

        // Assert
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void Renders_Menu_Items_Correctly()
    {
        // Arrange
        SetupAuth();
        var menuItems = new List<MenuItem>
        {
            new MenuItem(Guid.NewGuid(), "Pizza", 10.00m),
            new MenuItem(Guid.NewGuid(), "Burger", 5.00m) 
        };
        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);

        // Act
        var cut = Render<Menu>();

        // Assert
        Assert.Equal(2, cut.FindAll(".menu-item").Count);
        Assert.Contains("Pizza", cut.Markup);
        Assert.Contains("Burger", cut.Markup);
    }

    [Fact]
    public void AddToCart_Updates_Cart_Section()
    {
        // Arrange
        SetupAuth();
        var menuItems = new List<MenuItem>
        {
            new MenuItem(Guid.NewGuid(), "Pizza", 10.00m),
        };
        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);

        var cut = Render<Menu>();

        // Act
        cut.Find("button.btn-primary").Click(); // Click "Add to Cart"

        // Assert
        Assert.Contains("Your Cart", cut.Markup);
        Assert.Contains("Quantity: 1", cut.Markup);
        Assert.Contains("$10.00", cut.Markup); // Total
    }

    [Fact]
    public void GoToCheckout_Navigates_And_Updates_State()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        var menuItems = new List<MenuItem>
        {
            new MenuItem(Guid.NewGuid(), "Pizza", 10.00m),
        };
        _menuServiceMock.Setup(x => x.GetAllMenuItemsAsync()).ReturnsAsync(menuItems);
        var navMan = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Menu>();
        cut.Find("button.btn-primary").Click(); // Add to cart

        // Act
        cut.Find("button.btn-success").Click(); // Go to Checkout

        // Assert
        Assert.Equal("http://localhost/checkout", navMan.Uri);
        Assert.Equal(userId, _cartState.UserId);
        Assert.Single(_cartState.CartItems);
    }

    private void SetupAuth(Guid? userId = null)
    {
        var uid = userId ?? Guid.NewGuid();
        JSInterop.Setup<string>("eval", "document.cookie").SetResult("auth=token");
        _authServiceMock.Setup(x => x.GetTokenAsync(It.IsAny<string>())).Returns("token");
        _authServiceMock.Setup(x => x.ParseJwt("token")).Returns(new UserSession { Id = uid, Username =  "Test User", Email = "test@example.ro"});
    }
}