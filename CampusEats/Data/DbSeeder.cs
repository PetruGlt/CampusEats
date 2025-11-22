using CampusEats.Features.Menu;
using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(CampusEatsContext context)
    {
        // Check if we already have data
        if (await context.MenuItems.AnyAsync())
        {
            return; // Database already seeded
        }

        // Seed menu items
        var menuItems = new List<MenuItem>
        {
            new MenuItem(Guid.NewGuid(), "Classic Burger", 8.99m),
            new MenuItem(Guid.NewGuid(), "Cheeseburger", 9.99m),
            new MenuItem(Guid.NewGuid(), "Veggie Burger", 8.49m),
            new MenuItem(Guid.NewGuid(), "Margherita Pizza", 12.50m),
            new MenuItem(Guid.NewGuid(), "Pepperoni Pizza", 13.99m),
            new MenuItem(Guid.NewGuid(), "Caesar Salad", 6.99m),
            new MenuItem(Guid.NewGuid(), "Greek Salad", 7.49m),
            new MenuItem(Guid.NewGuid(), "French Fries", 3.99m),
            new MenuItem(Guid.NewGuid(), "Onion Rings", 4.49m),
            new MenuItem(Guid.NewGuid(), "Chicken Wings", 9.99m),
            new MenuItem(Guid.NewGuid(), "Pasta Carbonara", 11.99m),
            new MenuItem(Guid.NewGuid(), "Grilled Chicken Sandwich", 8.99m),
            new MenuItem(Guid.NewGuid(), "Fish and Chips", 10.99m),
            new MenuItem(Guid.NewGuid(), "Chocolate Cake", 5.99m),
            new MenuItem(Guid.NewGuid(), "Ice Cream Sundae", 4.99m),
            new MenuItem(Guid.NewGuid(), "Coffee", 2.99m),
            new MenuItem(Guid.NewGuid(), "Fresh Juice", 3.99m),
            new MenuItem(Guid.NewGuid(), "Soda", 1.99m)
        };

        context.MenuItems.AddRange(menuItems);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Database seeded with {menuItems.Count} menu items");
    }
}
