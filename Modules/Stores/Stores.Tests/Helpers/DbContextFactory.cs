using Microsoft.EntityFrameworkCore;
using Stores.Data;

namespace Stores.Tests.Helpers;

public static class DbContextFactory
{
    public static StoresDbContext Create()
    {
        var options = new DbContextOptionsBuilder<StoresDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        
        var context = new StoresDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}