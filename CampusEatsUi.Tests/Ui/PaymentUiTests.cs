using Bunit;
using CampusEatsUI.Models;
using CampusEatsUI.Models.Helpers;
using CampusEatsUI.Pages;
using CampusEatsUI.Services.Auth;
using CampusEatsUI.Services.Payment;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CampusEatsUI.UI;

public class PaymentUiTests : BunitContext
{
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IAuthenticationService> _authServiceMock;

    public PaymentUiTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _authServiceMock = new Mock<IAuthenticationService>();

        Services.AddSingleton(_paymentServiceMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
        Services.AddSingleton(new HttpClient());
    }

    [Fact]
    public void OnInitialized_Redirects_To_Login_If_No_Cookie()
    {
        // Arrange
        JSInterop.Setup<string>("eval", "document.cookie").SetResult(string.Empty);
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<Payment>();

        // Assert
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void Renders_Payment_History_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);

        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var payments = new List<PaymentHistoryResponse>
        {
            new PaymentHistoryResponse
            (
                paymentId,
                orderId,
                50,
                "usd",
                "Succeeded",
                userId,
                DateTime.Now,
                DateTime.Now,
                null,
                null
            )
        };

        _paymentServiceMock.Setup(x => x.GetPaymentHistoryAsync(userId.ToString(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(payments);

        // Act
        var cut = Render<Payment>();

        // Assert
        Assert.Contains(paymentId.ToString().Substring(0, 6), cut.Markup);
        Assert.Contains(orderId.ToString().Substring(0, 6), cut.Markup);
        Assert.Contains("$50.00", cut.Markup);
        Assert.Contains("Succeeded", cut.Markup);
    }

    [Fact]
    public void Renders_Empty_Table_When_No_Payments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupAuth(userId);

        _paymentServiceMock.Setup(x => x.GetPaymentHistoryAsync(userId.ToString(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ReturnsAsync(new List<PaymentHistoryResponse>());

        // Act
        var cut = Render<Payment>();

        // Assert
        Assert.Empty(cut.FindAll("tbody tr"));
    }

    private void SetupAuth(Guid userId)
    {
        JSInterop.Setup<string>("eval", "document.cookie").SetResult("auth=token");
        _authServiceMock.Setup(x => x.GetTokenAsync(It.IsAny<string>())).Returns("token");
        _authServiceMock.Setup(x => x.ParseJwt("token")).Returns(new UserSession { Id = userId, Username = "Test User", Email = "test@example.com" });
    }
}