

namespace Stores;

public static class StoresModule
{
    public static IServiceCollection AddStoresModule(this IServiceCollection services , IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddDbContext<StoresDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
        services.AddScoped<IStoreOwnershipChecker, StoreOwnershipChecker>();
        return services;
    }

    public static IApplicationBuilder UseStoresModule(this IApplicationBuilder app)
    {
        app.UseMigration<StoresDbContext>();
        return app;
    }
}