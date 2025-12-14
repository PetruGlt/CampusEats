using CampusEats.Data;
using CampusEats.Features.Menu;
using CampusEats.Features.Orders;
using CampusEats.Features.Kitchen;
using CampusEats.Features.Loyalty;
using CampusEats.Features.Payment;
using CampusEats.Features.Users;
using CampusEats.Features.Webhooks;
using CampusEats.Mappings;
using CampusEats.Middleware;
using CampusEats.Persistence;
using CampusEats.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Configure Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddScoped<IStripeClient>(sp => new StripeClient(builder.Configuration["Stripe:SecretKey"]));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        "v1", 
        new OpenApiInfo
        {
            Title = "CampusEats",
            Version = "v1",
            Description = "API for managing menu items.",
            Contact = new OpenApiContact
            {
                Name = "dev1: Galateanu Petru",
                Email = "galateanupetru152@campusEats.com",
                Url = new Uri("https://github.com/PetruGlt")
            }
        });
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<CampusEatsContext>(options =>
        options.UseInMemoryDatabase("CampusEats.TestDb"));
}
else
{
    builder.Services.AddDbContext<CampusEatsContext>(options =>
        options.UseSqlite("Data Source = CampusEats.db"));
}
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MenuItemMappingProfile>());

// Register User handlers
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetAllUsersHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<GetUserByCredentialHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();


// Register Menu handlers
builder.Services.AddScoped<CreateMenuItemHandler>();
builder.Services.AddScoped<GetAllMenuItemsHandler>();
builder.Services.AddScoped<GetByIdMenuItemHandler>();
builder.Services.AddScoped<UpdateMenuItemHandler>();
builder.Services.AddScoped<DeleteMenuItemHandler>();

// Register Order handlers
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetAllOrdersHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddScoped<CancelOrderHandler>();
builder.Services.AddScoped<GetOrderHistoryHandler>();
builder.Services.AddScoped<GetOrderStatisticsHandler>();
builder.Services.AddScoped<SearchOrdersHandler>();
builder.Services.AddScoped<GetOrderWaitTimeHandler>();

// Register Kitchen handlers
builder.Services.AddScoped<GetPendingOrdersHandler>();
builder.Services.AddScoped<UpdateOrderStatusHandler>();
builder.Services.AddScoped<GetKitchenDashboardHandler>();
builder.Services.AddScoped<BulkUpdateOrderStatusHandler>();
builder.Services.AddScoped<GetPopularItemsHandler>();

// Register Payment handlers
builder.Services.AddScoped<CreatePaymentHandler>();
builder.Services.AddScoped<GetPaymentHistoryHandler>();
builder.Services.AddScoped<GetPaymentByIdHandler>();
builder.Services.AddScoped<HandleStripeWebhook>();

// Register Loyalty services
builder.Services.AddScoped<LoyaltyService>();
builder.Services.AddScoped<GetUserPointsHandler>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateMenuItemValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateMenuItemValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderStatusValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateOrderStatusRequestBodyValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure db is created and seeded at runtime
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();
    context.Database.EnsureCreated();
    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CampusEats v1");
        c.RoutePrefix = string.Empty;
        c.DisplayOperationId();
    });
    app.MapOpenApi();
}
app.UseGlobalExceptionMiddleware();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// User endpoints

app.MapPost("/users/create", async (CreateUserRequest command, CreateUserHandler handler) =>
    await handler.Handler(command))
    .WithTags("Users")
    .WithName("CreateUser")
    .WithOpenApi();

app.MapGet("/users", async (GetAllUsersHandler handler) =>
    await handler.Handle(new GetAllUsersRequest()))
    .WithTags("Users")
    .WithName("GetAllUsers")
    .WithOpenApi();

app.MapGet("/users/{id:guid}", async (Guid id, GetUserByIdHandler handler) =>
    await handler.Handle(new GetUserByIdRequest(id)))
    .WithTags("Users")
    .WithName("GetUserById")
    .WithOpenApi();

app.MapPost("/users", async (GetUserByCredentialRequest command, GetUserByCredentialHandler handler) =>
    await handler.Handle(command))
    .WithTags("Users")
    .WithName("GetUserByCredential")
    .WithOpenApi();
app.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest command, UpdateUserHandler handler) =>
{
    var updated = command with { Id = id };
    var result = await handler.Handle(updated);
    return result;
})
.WithTags("Users")
.WithName("UpdateUser")
.WithOpenApi();

app.MapDelete("/users/{id:guid}", async (Guid id, DeleteUserHandler handler) =>
{
    await handler.Handle(new DeleteUserRequest(id));
})
.WithTags("Users")
.WithName("DeleteUser")
.WithOpenApi();


// Menu endpoints
app.MapPost("/menu", async (CreateMenuItemRequest command, CreateMenuItemHandler handler) => 
    await handler.Handle(command))
    .WithTags("Menu")
    .WithName("CreateMenuItem")
    .WithOpenApi();

app.MapGet("/menu", async (GetAllMenuItemsHandler handler) => 
    await handler.Handle(new GetAllMenuItemsRequest()))
    .WithTags("Menu")
    .WithName("GetAllMenuItems")
    .WithOpenApi();

app.MapGet("/menu/{id:guid}", async (Guid id, GetByIdMenuItemHandler handler) => 
    await handler.Handle(new GetByIdMenuItemRequest(id)))
    .WithTags("Menu")
    .WithName("GetMenuItemById")
    .WithOpenApi();

app.MapPut("/menu/{id:guid}", async (Guid id, UpdateMenuItemRequest command, UpdateMenuItemHandler handler) =>
{
    var updated = command with { Id = id };
    var result = await handler.Handle(updated);
    return result;
})
    .WithTags("Menu")
    .WithName("UpdateMenuItem")
    .WithOpenApi();

app.MapDelete("/menu/{id:guid}", async (Guid id, DeleteMenuItemHandler handler) => 
    await handler.Handle(new DeleteMenuItemRequest(id)))
    .WithTags("Menu")
    .WithName("DeleteMenuItem")
    .WithOpenApi();

// Order endpoints
app.MapPost("/orders", async (CreateOrderRequest request, CreateOrderHandler handler) =>
{
    var result = await handler.Handle(request);
    return Results.Created($"/orders/{result.Id}", result);
})
    .WithTags("Orders")
    .WithName("CreateOrder")
    .WithOpenApi();

app.MapGet("/orders", async (string? userId, GetAllOrdersHandler handler) =>
{
    var result = await handler.Handle(new GetAllOrdersRequest(userId));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("GetAllOrders")
    .WithOpenApi();

app.MapGet("/orders/{id:guid}", async (Guid id, GetOrderByIdHandler handler) =>
{
    var result = await handler.Handle(new GetOrderByIdRequest(id));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("GetOrderById")
    .WithOpenApi();

app.MapPut("/orders/{id:guid}/cancel", async (Guid id, CancelOrderHandler handler) =>
{
    var result = await handler.Handle(new CancelOrderRequest(id));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("CancelOrder")
    .WithOpenApi();

app.MapGet("/orders/history", async (DateTime? startDate, DateTime? endDate, string? userId, GetOrderHistoryHandler handler) =>
{
    var result = await handler.Handle(new GetOrderHistoryRequest(startDate, endDate, userId));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("GetOrderHistory")
    .WithOpenApi();

app.MapGet("/orders/statistics", async (GetOrderStatisticsHandler handler) =>
{
    var result = await handler.Handle(new GetOrderStatisticsRequest());
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("GetOrderStatistics")
    .WithOpenApi();

app.MapGet("/orders/search", async (string? query, string? status, SearchOrdersHandler handler) =>
{
    var result = await handler.Handle(new SearchOrdersRequest(query, status));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("SearchOrders")
    .WithOpenApi();

app.MapGet("/orders/{id:guid}/wait-time", async (Guid id, GetOrderWaitTimeHandler handler) =>
{
    var result = await handler.Handle(new GetOrderWaitTimeRequest(id));
    return Results.Ok(result);
})
    .WithTags("Orders")
    .WithName("GetOrderWaitTime")
    .WithOpenApi();

// Kitchen endpoints
app.MapGet("/kitchen/orders", async (GetPendingOrdersHandler handler) =>
{
    var result = await handler.Handle(new GetPendingOrdersRequest());
    return Results.Ok(result);
})
    .WithTags("Kitchen")
    .WithName("GetPendingOrders")
    .WithOpenApi();

app.MapPut("/kitchen/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusRequestBody body, UpdateOrderStatusHandler handler) =>
{
    var request = new UpdateOrderStatusRequest(id, body.Status);
    var result = await handler.Handle(request);
    return Results.Ok(result);
})
    .WithTags("Kitchen")
    .WithName("UpdateOrderStatus")
    .WithOpenApi();

app.MapGet("/kitchen/dashboard", async (GetKitchenDashboardHandler handler) =>
{
    var result = await handler.Handle(new GetKitchenDashboardRequest());
    return Results.Ok(result);
})
    .WithTags("Kitchen")
    .WithName("GetKitchenDashboard")
    .WithOpenApi();

app.MapPut("/kitchen/orders/bulk-update", async (BulkUpdateOrderStatusRequest request, BulkUpdateOrderStatusHandler handler) =>
{
    var result = await handler.Handle(request);
    return Results.Ok(result);
})
    .WithTags("Kitchen")
    .WithName("BulkUpdateOrderStatus")
    .WithOpenApi();

app.MapGet("/kitchen/popular-items", async (int? topN, GetPopularItemsHandler handler) =>
{
    var result = await handler.Handle(new GetPopularItemsRequest(topN));
    return Results.Ok(result);
})
    .WithTags("Kitchen")
    .WithName("GetPopularItems")
    .WithOpenApi();

// Payment endpoints
app.MapPost("/payments/create-checkout", async (CreatePaymentRequest request, CreatePaymentHandler handler) =>
{
    var result = await handler.Handle(request);
    return Results.Ok(result);
})
    .WithTags("Payment")
    .WithName("CreateCheckoutSession")
    .WithOpenApi();

app.MapGet("/payments/history", async (string? userId, DateTime? startDate, DateTime? endDate, string? status, GetPaymentHistoryHandler handler) =>
{
    PaymentStatus? paymentStatus = null;
    if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
    {
        paymentStatus = parsedStatus;
    }
    
    var request = new GetPaymentHistoryRequest(userId, startDate, endDate, paymentStatus);
    var result = await handler.Handle(request);
    return Results.Ok(result);
})
    .WithTags("Payment")
    .WithName("GetPaymentHistory")
    .WithOpenApi();

app.MapGet("/payments/{id:guid}", async (Guid id, GetPaymentByIdHandler handler) =>
{
    var result = await handler.Handle(new GetPaymentByIdRequest(id));
    return Results.Ok(result);
})
    .WithTags("Payment")
    .WithName("GetPaymentById")
    .WithOpenApi();

app.MapPost("/webhooks/stripe", async (HttpContext context, HandleStripeWebhook handler) =>
{
    var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var signature = context.Request.Headers["Stripe-Signature"].ToString();

    var success = await handler.Handle(json, signature);
    return success ? Results.Ok() : Results.BadRequest();
});

// Loyalty endpoint
app.MapGet("/loyalty/{userId}", async (string userId, GetUserPointsHandler handler) =>
{
    var result = await handler.Handle(new GetUserPointsRequest(userId));
    return Results.Ok(result);
})
    .WithTags("Loyalty")
    .WithName("GetUserLoyaltyPoints")
    .WithOpenApi();

app.Run();
