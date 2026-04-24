using Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Tests.Helpers;

public static class DbContextFactory
{
    public static IdentityDbContext Create()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new IdentityDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
