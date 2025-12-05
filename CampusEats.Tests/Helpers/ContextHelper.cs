using CampusEats.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Tests.Helpers;

public class ContextHelper
{
    public static CampusEatsContext CreateInMemoryDBContext()
    {
        var options = new DbContextOptionsBuilder<CampusEatsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new CampusEatsContext(options);
        return dbContext;
    }
}