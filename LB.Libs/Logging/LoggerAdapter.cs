using Microsoft.Extensions.Logging;

namespace LB.Libs.Logging;

/// <summary>
/// Адаптер для интеграции современного Microsoft.Extensions.Logging с существующей кодовой базой.
/// Предоставляет bridge между новым ILogger и legacy Logger API.
/// </summary>
public class LoggerAdapter
{
    private readonly ILogger _logger;

    public LoggerAdapter(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Error(object message)
    {
        if (ShouldSkip(message))
            return;

        if (message is Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
        else
        {
            _logger.LogError("{Message}", message);
        }
    }

    public void Error(object message, Exception exception)
    {
        if (ShouldSkip(message))
            return;

        _logger.LogError(exception, "{Message}", message);
    }

    public void ErrorFormatted(string format, params object[] args)
    {
        _logger.LogError(format, args);
    }

    public void Fatal(object message)
    {
        if (ShouldSkip(message))
            return;

        if (message is Exception ex)
        {
            _logger.LogCritical(ex, ex.Message);
        }
        else
        {
            _logger.LogCritical("{Message}", message);
        }
    }

    public void Fatal(object message, Exception exception)
    {
        if (ShouldSkip(message))
            return;

        _logger.LogCritical(exception, "{Message}", message);
    }

    public void FatalFormatted(string format, params object[] args)
    {
        _logger.LogCritical(format, args);
    }

    public void Info(object message)
    {
        _logger.LogInformation("{Message}", message);
    }

    public void InfoFormatted(string format, params object[] args)
    {
        _logger.LogInformation(format, args);
    }

    public void Debug(object message)
    {
        _logger.LogDebug("{Message}", message);
    }

    public void DebugFormatted(string format, params object[] args)
    {
        _logger.LogDebug(format, args);
    }

    public void Warning(object message)
    {
        _logger.LogWarning("{Message}", message);
    }

    public void WarningFormatted(string format, params object[] args)
    {
        _logger.LogWarning(format, args);
    }

    private static bool ShouldSkip(object message)
    {
        return message is Exception ex && ex.InnerException is UserWantExit;
    }
}
