using Microsoft.Extensions.Logging;

namespace LB.Libs.Logging;

/// <summary>
/// Провайдер для управления экземплярами LoggerAdapter.
/// Позволяет интегрировать ILoggerFactory с существующей кодовой базой.
/// </summary>
public static class LoggerServiceProvider
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Инициализирует провайдер с указанной фабрикой логирования.
    /// </summary>
    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Создает адаптер логирования для указанной категории.
    /// </summary>
    public static LoggerAdapter CreateLogger(string category)
    {
        if (_loggerFactory == null)
        {
            throw new InvalidOperationException(
                "LoggerServiceProvider must be initialized with ILoggerFactory before creating loggers. " +
                "Call LoggerServiceProvider.Initialize(loggerFactory) during application startup.");
        }

        var logger = _loggerFactory.CreateLogger(category);
        return new LoggerAdapter(logger);
    }

    /// <summary>
    /// Создает адаптер логирования для указанного типа.
    /// </summary>
    public static LoggerAdapter CreateLogger<T>()
    {
        return CreateLogger(typeof(T).FullName ?? typeof(T).Name);
    }

    /// <summary>
    /// Сбрасывает инициализацию провайдера (используется в тестах).
    /// </summary>
    internal static void Reset()
    {
        _loggerFactory = null;
    }
}
