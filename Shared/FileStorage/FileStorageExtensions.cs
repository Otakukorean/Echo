using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.FileStorage;

namespace Shared.FileStorage;

public static class FileStorageExtensions
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new BlobStorageSettings();
        configuration.GetSection(BlobStorageSettings.SectionName).Bind(settings);

        services.AddSingleton(settings);
        services.AddSingleton(new BlobServiceClient(settings.ConnectionString));
        services.AddSingleton<FileValidator>();
        services.AddSingleton<IFileStorageService, BlobStorageService>();

        return services;
    }
}
