/*
 * TODO: Refactor the code:
 * - Model
 * - Service
 * - Pages
 */

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CampusEatsUI;
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
builder.Services.AddSingleton<IKitchenService, KitchenService>();
builder.Services.AddSingleton<IMenuService, MenuService>();
builder.Services.AddSingleton<IOrderService, OrderServices>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IUserLoyaltyService, UserLoyaltyService>();
builder.Services.AddSingleton<IUserService, UserService>();

//Auxiliary Services (Authentication, Stripe, Order Images Handling) 
await builder.Build().RunAsync();