using Microsoft.Extensions.Http.Resilience;
using NewsWave.Abstractions;
using NewsWave.Services;
using Polly;
using System.Net.Mail;

namespace NewsWave.Configuration;

/// <summary>
/// Регистрация email сервисов с resilience
/// </summary>
public static class EmailServiceCollectionExtensions
{
    /// <summary>
    /// Добавить email sender с Polly resilience
    /// </summary>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        bool useResilience = true)
    {
        if (useResilience)
        {
            // Регистрация с Polly resilience decorator
            services.AddScoped<SmtpEmailSender>();
            services.AddScoped<IEmailSender>(provider =>
            {
                var inner = provider.GetRequiredService<SmtpEmailSender>();
                var logger = provider.GetRequiredService<ILogger<ResilientEmailSender>>();
                return new ResilientEmailSender(inner, logger);
            });
        }
        else
        {
            // Регистрация без resilience (для тестирования)
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        return services;
    }
}
