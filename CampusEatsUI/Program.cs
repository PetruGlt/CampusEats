
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CampusEatsUI;
using CampusEatsUI.Services.Auth;
using CampusEatsUI.Services.Helpers;
using CampusEatsUI.Services.Kitchen;
using CampusEatsUI.Services.Menu;
using CampusEatsUI.Services.Orders;
using CampusEatsUI.Services.Payment;
using CampusEatsUI.Services.UserLoyalty;
using CampusEatsUI.Services.Users;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Main Services
builder.Services.AddScoped<IKitchenService, KitchenService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderServices>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserLoyaltyService, UserLoyaltyService>();
builder.Services.AddScoped<IUserService, UserService>();

//Auxiliary Services (Authentication, Stripe, Order Images Handling) 

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CartState>();

await builder.Build().RunAsync();