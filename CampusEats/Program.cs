using CampusEats.Features.Menu;
using CampusEats.Mappings;
using CampusEats.Middleware;
using CampusEats.Persistence;
using CampusEats.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<CreateMenuItemHandler>();
builder.Services.AddScoped<GetAllMenuItemsHandler>();
builder.Services.AddScoped<GetByIdMenuItemHandler>();
builder.Services.AddScoped<UpdateMenuItemHandler>();
builder.Services.AddScoped<DeleteMenuItemHandler>();

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


app.MapPost("/menu", async (CreateMenuItemRequest command, CreateMenuItemHandler handler) => 
    await handler.Handle(command));
app.MapGet("/menu", async (GetAllMenuItemsHandler handler) => 
    await handler.Handle(new GetAllMenuItemsRequest()));
app.MapGet("/menu/{id:guid}", async (Guid id, GetByIdMenuItemHandler handler) => 
    await handler.Handle(new GetByIdMenuItemRequest(id)));
app.MapPut("/menu/{id:guid}", async (Guid id, UpdateMenuItemRequest command, UpdateMenuItemHandler handler) =>
{
    var updated = command with { Id = id };
    var result = await handler.Handle(updated);
    return result;
});
app.MapDelete("/menu/{id:guid}", async (Guid id, DeleteMenuItemHandler handler) => 
    await handler.Handle(new DeleteMenuItemRequest(id)));



app.Run();
