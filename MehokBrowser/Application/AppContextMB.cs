using MehokBrowser.Configs.Cfg;
using MehokBrowser.Configs.Forms;
using CfgApp = LB.Libs.CfgApp;
using CfgCoreConnection = LB.Libs.CfgCoreConnection;
using IniHelper = LB.Libs.IniHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using LB.Libs.Utils;

namespace MeshokBrowser;

/// <summary>
/// Application context для MehokBrowser с интеграцией современного DI контейнера
/// </summary>
public class AppContextMB : AppContext<AppContextMB>
{
    /// <summary>
    /// Доступ к сервисам из DI контейнера (заполняется Program.cs)
    /// </summary>
    public static IServiceProvider? Services => Program.ServiceProvider;

    /// <summary>
    /// Получение сервиса из DI контейнера
    /// </summary>
    /// <typeparam name="T">Тип сервиса</typeparam>
    /// <returns>Экземпляр сервиса или null если сервис недоступен</returns>
    public static T? GetService<T>() where T : class
    {
        return Services?.GetService<T>();
    }

    /// <summary>
    /// Получение обязательного сервиса из DI контейнера (выбрасывает исключение если не найден)
    /// </summary>
    /// <typeparam name="T">Тип сервиса</typeparam>
    /// <returns>Экземпляр сервиса</returns>
    public static T GetRequiredService<T>() where T : notnull
    {
        if (Services == null)
            throw new InvalidOperationException("Service provider is not initialized. Call Program.Main() first.");

        return Services.GetRequiredService<T>();
    }

    protected override void InitializeSomething()
    {
        var logger = GetService<ILogger<AppContextMB>>();
        logger?.LogInformation("AppContextMB initialization started");

        try
        {
            // Первый запуск собирает старые разрозненные INI в один файл рядом с exe.
            IniFile.MigrateLegacyFiles();
            logger?.LogInformation("Legacy INI files migration completed");

            // WebView2 is initialized by GhBrowser when its handle is created.
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error during AppContextMB initialization");
            throw;
        }
    }

    public override Form GetMainForm()
    {
        var logger = GetService<ILogger<AppContextMB>>();
        logger?.LogDebug("Creating main form");

        // Пытаемся получить форму из DI, иначе создаём напрямую
        return GetService<MainMeshok>() ?? new MainMeshok();
    }

    public override Form GetLoginForm()
    {
        var logger = GetService<ILogger<AppContextMB>>();
        logger?.LogDebug("Creating login form");

        return new LoginFormIShop();
    }

    public override CfgCoreConnection GetConnectionSetting()
    {
        var cfg = IniHelper.Cfg<CfgIShop>();
        if (cfg == null)
            cfg = new CfgIShop();
        return cfg;
    }

    public override CfgForm CreateConnectForm()
    {
        return new CfgFormIShop();
    }

    public override CfgApp GetCfgApp()
    {
        return new CfgApp();
    }
}
