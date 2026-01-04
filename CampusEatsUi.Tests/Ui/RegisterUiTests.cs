using Bunit;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CampusEatsUI.UI;

public class RegisterUiTests : BunitContext
{
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public RegisterUiTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        Services.AddSingleton(_authServiceMock.Object);
    }

    [Fact]
    public void Renders_RegisterForm_Correctly()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);

        // Act
        var cut = Render<Register>();

        // Assert
        Assert.NotNull(cut.Find("form"));
        Assert.NotNull(cut.Find("#username"));
        Assert.NotNull(cut.Find("#email"));
        Assert.NotNull(cut.Find("#password"));
        Assert.NotNull(cut.Find("#confirmPassword"));
        Assert.Contains("Register", cut.Find("button[type='submit']").TextContent);
    }

    [Fact]
    public void OnInitialized_Redirects_When_Token_Exists()
    {
        // Arrange
        var cookieString = "authToken=existing_token";
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(cookieString);
        
        _authServiceMock.Setup(x => x.GetTokenAsync(cookieString)).Returns("existing_token");
        
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<Register>();

        // Assert
        Assert.Equal("http://localhost/", navMan.Uri);
    }

    [Fact]
    public void ValidSubmit_Registers_And_Redirects()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var cut = Render<Register>();

        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";

        _authServiceMock.Setup(x => x.RegisterAsync(username, email, password))
            .Returns(Task.CompletedTask);

        // Act
        cut.Find("#username").Change(username);
        cut.Find("#email").Change(email);
        cut.Find("#password").Change(password);
        cut.Find("#confirmPassword").Change(password);
        
        cut.Find("form").Submit();

        // Assert
        _authServiceMock.Verify(x => x.RegisterAsync(username, email, password), Times.Once);
        var navMan = Services.GetRequiredService<NavigationManager>();
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void InvalidSubmit_PasswordMismatch_Displays_ErrorMessage()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var cut = Render<Register>();

        // Act
        cut.Find("#username").Change("user");
        cut.Find("#email").Change("test@example.com");
        cut.Find("#password").Change("password123");
        cut.Find("#confirmPassword").Change("mismatch");
        
        cut.Find("form").Submit();

        // Assert
        cut.WaitForState(() => cut.FindAll(".alert-danger").Count > 0);
        Assert.Contains("Passwords did not match", cut.Find(".alert-danger").TextContent);
        
        _authServiceMock.Verify(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}