/*
 *Logic:
 * - Creates a new session with new user GUID (For persistence, we shall create a cookie containing the user guid)
 * - User Selects menu Items, The items will be added to a newly entry with user id and menu item id
 * - User checkouts the menu and will be redirected to payment page using stripe (Paul better explain that)
 * - After the payment is successful, the order will be added to the orders table with status as "Pending"
 */

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CampusEatsUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();