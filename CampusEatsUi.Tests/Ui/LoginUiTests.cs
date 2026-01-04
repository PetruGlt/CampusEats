using Bunit;
using CampusEatsUI.Models.Auth;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CampusEatsUI.UI;

public class LoginUiTests : BunitContext
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
 
         public LoginUiTests()
         {
             _authServiceMock = new Mock<IAuthenticationService>();
             Services.AddSingleton(_authServiceMock.Object);
         }
 
         [Fact]
         public void Renders_LoginForm_Correctly()
         {
             // Arrange
             // Mock the JS call in OnInitializedAsync to return empty cookie
             JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
 
             // Act
             var cut = Render<Login>();
 
             // Assert
             Assert.NotNull(cut.Find("form"));
             Assert.NotNull(cut.Find("#email"));
             Assert.NotNull(cut.Find("#password"));
             Assert.Contains("Login", cut.Find("button[type='submit']").TextContent);
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
             Render<Login>();
 
             // Assert
             Assert.Equal("http://localhost/", navMan.Uri);
         }
 
         [Fact]
         public void ValidSubmit_LogsIn_And_Redirects()
         {
             // Arrange
             JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
             var cut = Render<Login>();
 
             var email = "test@example.com";
             var password = "password123";
             var token = "new_token";
 
             // Assuming LoginResponse is the return type of LoginAsync
             _authServiceMock.Setup(x => x.LoginAsync(email, password))
                 .ReturnsAsync(new AuthResponse(token));
 
             // Act
             cut.Find("#email").Change(email);
             cut.Find("#password").Change(password);
             
             // Setup expectation for setting the cookie via JS
             var jsCookiePlan = JSInterop.SetupVoid("eval", invocation => 
                 invocation.Arguments.Count > 0 && 
                 invocation.Arguments[0].ToString().Contains($"authToken={token}"));
 
             cut.Find("form").Submit();
 
             // Assert
             Assert.Equal(1, jsCookiePlan.Invocations.Count);
             var navMan = Services.GetRequiredService<NavigationManager>();
             Assert.Equal("http://localhost/", navMan.Uri);
         }
 
         [Fact]
         public void InvalidSubmit_Displays_ErrorMessage()
         {
             // Arrange
             JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
             var cut = Render<Login>();
 
             _authServiceMock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(new AuthResponse(string.Empty));
 
             // Act
             cut.Find("#email").Change("test@example.com");
             cut.Find("#password").Change("wrong_password");
             cut.Find("form").Submit();
 
             // Assert
             cut.WaitForState(() => cut.FindAll(".alert-danger").Count > 0);
             Assert.Contains("Something went wrong", cut.Find(".alert-danger").TextContent);
         }
}