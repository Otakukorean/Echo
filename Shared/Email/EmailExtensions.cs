using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Email;

namespace Shared.Email;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new EmailSettings();
        configuration.GetSection(EmailSettings.SectionName).Bind(settings);

        services.AddSingleton(settings);
        services.AddTransient<IEmailService, SmtpEmailService>();

        return services;
    }
}
