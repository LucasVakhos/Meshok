using Microsoft.Extensions.Logging;

namespace MehokBrowser.Services;

/// <summary>
/// Обёртка для логирования, предоставляющая type-safe доступ к logger
/// </summary>
public interface ILoggingService
{
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception? exception, string message, params object[] args);
    void LogCritical(Exception? exception, string message, params object[] args);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(Exception? exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogCritical(Exception? exception, string message, params object[] args)
    {
        _logger.LogCritical(exception, message, args);
    }
}
