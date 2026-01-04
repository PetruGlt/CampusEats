using Bunit;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CampusEatsUI.Tests.Pages
{
    public class HomeTests : TestContext
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public HomeTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            Services.AddSingleton(_authServiceMock.Object);
        }

        [Fact]
        public void Renders_Unauthenticated_View_When_No_Cookie_Exists()
        {
            // Arrange
            JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);

            // Act
            var cut = RenderComponent<Home>();

            // Assert
            Assert.NotNull(cut.Find("a[href='login']"));
            Assert.NotNull(cut.Find("a[href='register']"));
            Assert.Empty(cut.FindAll("button.btn-danger")); // Logout button should not exist
        }

        [Fact]
        public void Renders_Authenticated_View_When_Valid_Cookie_Exists()
        {
            // Arrange
            var cookie = "auth=valid_token";
            var token = "valid_token";
            var username = "HungryStudent";
            var userSession = new UserSession { Username = username };

            JSInterop.Setup<string>("eval", "document.cookie").SetResult(cookie);
            
            _authServiceMock.Setup(x => x.GetTokenAsync(cookie))
                .ReturnsAsync(token);
            
            _authServiceMock.Setup(x => x.ParseJwt(token))
                .Returns(userSession);

            // Act
            var cut = RenderComponent<Home>();

            // Assert
            cut.Find("h5").MarkupMatches($"<h5>Welcome back {username}</h5>");
            Assert.NotNull(cut.Find("button.btn-danger")); // Logout button
            Assert.Empty(cut.FindAll("a[href='login']")); // Login link should not exist
        }

        [Fact]
        public void Renders_Unauthenticated_View_When_Token_Parsing_Fails()
        {
            // Arrange
            var cookie = "auth=bad_token";
            var token = "bad_token";

            JSInterop.Setup<string>("eval", "document.cookie").SetResult(cookie);
            
            _authServiceMock.Setup(x => x.GetTokenAsync(cookie))
                .ReturnsAsync(token);
            
            _authServiceMock.Setup(x => x.ParseJwt(token))
                .Throws(new Exception("Invalid Token"));

            // Act
            var cut = RenderComponent<Home>();

            // Assert
            Assert.NotNull(cut.Find("a[href='login']"));
        }

        [Fact]
        public async Task Logout_Button_Calls_Service_And_Updates_UI()
        {
            // Arrange
            var cookie = "auth=token";
            var userSession = new UserSession { Username = "TestUser" };

            JSInterop.Setup<string>("eval", "document.cookie").SetResult(cookie);
            _authServiceMock.Setup(x => x.GetTokenAsync(cookie)).ReturnsAsync("token");
            _authServiceMock.Setup(x => x.ParseJwt("token")).Returns(userSession);

            var cut = RenderComponent<Home>();

            // Act
            var logoutBtn = cut.Find("button.btn-danger");
            await logoutBtn.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

            // Assert
            _authServiceMock.Verify(x => x.LogoutAsync(), Times.Once);
            // Verify UI reverted to login state (checking for Login link)
            cut.WaitForState(() => cut.FindAll("a[href='login']").Any());
        }
    }
}