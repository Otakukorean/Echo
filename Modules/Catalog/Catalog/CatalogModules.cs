using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog;

public static class CatalogModules
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        return services;
    }

    public static IApplicationBuilder UseCatalogModule(this IApplicationBuilder app)
    {
        return app;
    }
}