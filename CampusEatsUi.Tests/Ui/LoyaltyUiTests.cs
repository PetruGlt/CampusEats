using Bunit;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using CampusEatsUI.Services.UserLoyalty;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CampusEatsUI.UI;

public class LoyaltyUiTests : BunitContext
{
    private readonly Mock<IUserLoyaltyService> _loyaltyServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public LoyaltyUiTests()
    {
        _loyaltyServiceMock = new Mock<IUserLoyaltyService>();
        _authServiceMock = new Mock<IAuthenticationService>();

        Services.AddSingleton(_loyaltyServiceMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
    }

    [Fact]
    public void OnInitialized_Redirects_To_Login_If_No_Cookie()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<Loyalty>();

        // Assert
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void Renders_Points_Correctly_When_User_Has_Points()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);

        var points = new UserPoints(userId, 150, DateTime.Now);
        _loyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(points);

        // Act
        var cut = Render<Loyalty>();

        // Assert
        Assert.Contains("You have 150 😃", cut.Markup);
        Assert.Contains("Use your loyalty points to get 25% discount", cut.Markup);
    }

    [Fact]
    public void Renders_No_Points_Message_When_Points_Are_Zero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);

        var points = new UserPoints(userId, 0, DateTime.Now);
        _loyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(points);

        // Act
        var cut = Render<Loyalty>();

        // Assert
        Assert.Contains("You have no points ☹️", cut.Markup);
    }

    [Fact]
    public void Menu_Button_Navigates_To_Menu()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);
        
        var points = new UserPoints(userId, 0, DateTime.Now);
        _loyaltyServiceMock.Setup(x => x.GetUserLoyaltyPointsAsync(userId))
            .ReturnsAsync(points);

        var navMan = Services.GetRequiredService<NavigationManager>();
        var cut = Render<Loyalty>();

        // Act
        cut.Find("button.btn-primary").Click();

        // Assert
        Assert.Equal("http://localhost/menu", navMan.Uri);
    }

    private void SetupAuth(Guid userId)
    {
        JSInterop.Setup<string>("eval", "document.cookie").SetResult("auth=token");
        _authServiceMock.Setup(x => x.GetTokenAsync(It.IsAny<string>())).Returns("token");
        _authServiceMock.Setup(x => x.ParseJwt("token")).Returns(new UserSession { Id = userId, Username = "Test User", Email = "test@example.com" });
    }
}