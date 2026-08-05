using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MeshokBrowser.Common;
using MeshokBrowser.Data;
using MehokBrowser.Services;
using System;
using System.IO;

namespace MeshokBrowser;

static class Program
{
    /// <summary>
    /// Service provider для доступа из legacy кода
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Главная точка входа для приложения.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Создание и запуск host
        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        // Получаем logger и настраиваем глобальную обработку ошибок
        var logger = ServiceProvider.GetRequiredService<ILogger<AppContextMB>>();
        GlobalExceptionHandler.Configure(logger);

        logger.LogInformation("MehokBrowser started");

        try
        {
            // Запуск LB.Libs AppContext (сохраняем совместимость с legacy framework)
            AppContextMB.RunInstance();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error during application execution");
            throw;
        }
        finally
        {
            logger.LogInformation("MehokBrowser stopped");
        }
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Настройка конфигурации
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                // Регистрация инфраструктурных сервисов
                services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ILoggingService, LoggingService>();

                // Регистрация репозиториев
                services.AddSingleton<IMessageSettingsRepository, DapperMessageSettingsRepository>();
                // DapperLookupRepository остаётся static для совместимости, но можно добавить wrapper при необходимости

                // Регистрация форм (transient - создаём новые инстансы при каждом запросе)
                services.AddTransient<MainMeshok>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                // Добавление log4net через Microsoft.Extensions.Logging
                logging.AddLog4Net("log4net.config");

                // Добавление console logging для development
                if (context.HostingEnvironment.IsDevelopment())
                {
                    logging.AddConsole();
                    logging.AddDebug();
                }

                logging.SetMinimumLevel(LogLevel.Information);
            })
            .UseConsoleLifetime(options => options.SuppressStatusMessages = true);
    }
}
