using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Pdf;

namespace Shared.Pdf;

public static class PdfExtensions
{
    public static IServiceCollection AddPdfService(this IServiceCollection services)
    {
        services.AddSingleton<IPdfService, QuestPdfService>();
        return services;
    }
}
