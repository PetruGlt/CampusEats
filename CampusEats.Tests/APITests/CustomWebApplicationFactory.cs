using CampusEats.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace CampusEats.Tests.APITests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<CampusEatsContext>) ||
                d.ServiceType == typeof(CampusEatsContext) ||
                (d.ImplementationType == typeof(CampusEatsContext))
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<CampusEatsContext>(options => options.UseInMemoryDatabase(dbName));
        });
    }
}