using CampusEats.Features.Kitchen;
using CampusEats.Features.Menu;
using CampusEats.Features.Orders;
using CampusEats.Features.Payment;
using CampusEats.Mappings;
using CampusEats.Middleware;
using CampusEats.Persistence;
using CampusEats.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Stripe;
using MediatR;

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
builder.Services.AddDbContext<CampusEatsContext>(options =>
    options.UseSqlite("Data Source = CampusEats.db"));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MenuItemMappingProfile>());
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


builder.Services.AddScoped<CreateMenuItemHandler>();
builder.Services.AddScoped<GetAllMenuItemsHandler>();
builder.Services.AddScoped<GetByIdMenuItemHandler>();
builder.Services.AddScoped<UpdateMenuItemHandler>();
builder.Services.AddScoped<DeleteMenuItemHandler>();

builder.Services.AddScoped<PaymentHandler>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateMenuItemValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateMenuItemValidator>();
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

// Ensure db is created at runtime
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();
    context.Database.EnsureCreated();
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


app.MapPost("/payment", async (PaymentRequest request, PaymentHandler handler) =>
{
    var paymentIntentId = await handler.CreateCheckoutSession(request);
    return Results.Ok(new { PaymentIntentId = paymentIntentId });
});

var menuGroup = app.MapGroup("/menu").WithTags("Menu");
menuGroup.MapPost("/", async (CreateMenuItemRequest command, IMediator mediator) => await mediator.Send(command));
menuGroup.MapGet("/", async (IMediator mediator) => await mediator.Send(new GetAllMenuItemsRequest()));
menuGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) => await mediator.Send(new GetByIdMenuItemRequest(id)));
menuGroup.MapPut("/{id:guid}", async (Guid id, UpdateMenuItemRequest command, IMediator mediator) =>
{
    var updated = command with { Id = id };
    return await mediator.Send(updated);
});
menuGroup.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) => await mediator.Send(new DeleteMenuItemRequest(id)));

var ordersGroup = app.MapGroup("/orders").WithTags("Orders");
ordersGroup.MapPost("/", async (PlaceOrderCommand req, IMediator mediator) => await mediator.Send(req));
ordersGroup.MapPut("/{orderId:guid}/cancel", async (Guid orderId, IMediator mediator) => await mediator.Send(new CancelOrderCommand(orderId)));
ordersGroup.MapGet("/{orderId:guid}", async (Guid orderId, IMediator mediator) => await mediator.Send(new GetOrderByIdQuery(orderId)));
app.MapGetOrdersByCustomer();
app.MapGetAllOrders();

var kitchenGroup = app.MapGroup("/kitchen").WithTags("Kitchen");
kitchenGroup.MapGet("/pending", async (IMediator mediator) => await mediator.Send(new ViewPendingOrdersQuery()));
kitchenGroup.MapPut("/orders/{orderId:guid}/status", async (Guid orderId, UpdateOrderStatusRequest req, IMediator mediator) => await mediator.Send(new UpdateOrderStatusCommand(orderId, req.NewStatus)));
kitchenGroup.MapGet("/daily-report", async (IMediator mediator) => await mediator.Send(new GetDailyReportQuery()));

app.Run();
