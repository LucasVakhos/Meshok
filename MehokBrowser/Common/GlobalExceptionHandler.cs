using LB.Libs;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MeshokBrowser.Common;

public static class GlobalExceptionHandler
{
    private static ILogger? _logger;

    /// <summary>
    /// Настройка глобальных обработчиков исключений
    /// </summary>
    /// <param name="logger">Опциональный logger для интеграции с Microsoft.Extensions.Logging</param>
    public static void Configure(ILogger? logger = null)
    {
        _logger = logger;

        // Обработчик для UI потока (Windows Forms)
        Application.ThreadException += (sender, e) =>
        {
            LogException("UI Thread Exception", e.Exception, LogLevel.Error);
            HandleException(e.Exception, "Ошибка в UI потоке");
        };

        // Обработчик для неперехваченных исключений в других потоках
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var exception = e.ExceptionObject as Exception;
            LogException("Unhandled Exception", exception, LogLevel.Critical);

            if (e.IsTerminating)
            {
                HandleException(exception, "Критическая ошибка", true);
            }
            else
            {
                HandleException(exception, "Необработанное исключение");
            }
        };

        // Обработчик для Task исключений
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            LogException("Unobserved Task Exception", e.Exception, LogLevel.Error);
            HandleException(e.Exception, "Ошибка в асинхронной операции");
            e.SetObserved(); // Предотвращаем падение приложения
        };
    }

    private static void LogException(string context, Exception? exception, LogLevel level)
    {
        if (exception == null)
        {
            Logger.Error($"{context}: Unknown exception");
            _logger?.Log(level, $"{context}: Unknown exception");
            return;
        }

        // Логируем через LB.Libs Logger (для совместимости)
        if (level == LogLevel.Critical)
        {
            Logger.Fatal(context, exception);
        }
        else
        {
            Logger.Error(context, exception);
        }

        // Логируем через Microsoft.Extensions.Logging (если доступен)
        _logger?.Log(level, exception, context);
    }

    private static void HandleException(Exception? exception, string title, bool isFatal = false)
    {
        if (exception == null)
        {
            Logger.Error($"{title}: Unknown exception");
            _logger?.LogError($"{title}: Unknown exception");
            return;
        }

        var message = $"{title}\n\n{exception.Message}\n\nДополнительная информация:\n{exception.GetType().Name}";

        if (exception.InnerException != null)
        {
            message += $"\n\nВнутреннее исключение:\n{exception.InnerException.Message}";
        }

        // В debug режиме показываем stack trace
        if (Debugger.IsAttached)
        {
            message += $"\n\nStack Trace:\n{exception.StackTrace}";
        }

        try
        {
            DlgHelper.DlgError(message);
        }
        catch
        {
            // Если не удалось показать диалог, выводим в консоль
            Console.WriteLine(message);
        }

        if (isFatal)
        {
            _logger?.LogCritical(exception, "Application terminating due to fatal error");
            Environment.Exit(1);
        }
    }
}
