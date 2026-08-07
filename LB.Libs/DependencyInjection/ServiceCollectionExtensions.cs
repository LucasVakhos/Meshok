using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LB.Libs.DependencyInjection;

/// <summary>
/// Extension methods для регистрации LB.Libs сервисов в DI контейнере.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует базовые сервисы LB.Libs в DI контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddLBLibsCore(this IServiceCollection services)
    {
        // Регистрация провайдера логирования
        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            Logging.LoggerServiceProvider.Initialize(loggerFactory);
            return Logging.LoggerServiceProvider.CreateLogger("LB.Libs");
        });

        return services;
    }

    /// <summary>
    /// Регистрирует конфигурационные сервисы LB.Libs.
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddLBLibsConfiguration(this IServiceCollection services)
    {
        // Регистрация IniFile как singleton для работы с конфигурацией
        // (будет добавлено позже при рефакторинге IniFile)

        return services;
    }

    /// <summary>
    /// Регистрирует все сервисы LB.Libs в DI контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddLBLibs(this IServiceCollection services)
    {
        services.AddLBLibsCore();
        services.AddLBLibsConfiguration();

        return services;
    }
}
