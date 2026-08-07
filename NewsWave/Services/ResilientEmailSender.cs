using Microsoft.Extensions.Options;
using NewsWave.Abstractions;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Mail;

namespace NewsWave.Services;

/// <summary>
/// Decorator для IEmailSender с Polly resilience policies
/// </summary>
public sealed class ResilientEmailSender : IEmailSender
{
    private readonly IEmailSender _inner;
    private readonly ILogger<ResilientEmailSender> _logger;
    private readonly ResiliencePipeline<EmailSendResult> _pipeline;

    public ResilientEmailSender(
        IEmailSender inner,
        ILogger<ResilientEmailSender> logger)
    {
        _inner = inner;
        _logger = logger;
        _pipeline = BuildResiliencePipeline();
    }

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(
                async ct => await _inner.SendAsync(message, ct),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resilience pipeline failed for email: {To}", message.To);
            return new EmailSendResult(false, $"Resilience pipeline error: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        // Batch отправка не использует resilience pipeline для избежания chaining
        // Каждое письмо обрабатывается индивидуально через SendAsync с resilience
        var results = new List<EmailSendResult>();

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var result = await SendAsync(message, cancellationToken);
            results.Add(result);

            // Небольшая задержка между отправками
            if (result.Success)
            {
                await Task.Delay(150, cancellationToken);
            }
        }

        return results;
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        // Test connection не использует resilience - это diagnostic метод
        return _inner.TestConnectionAsync(cancellationToken);
    }

    private ResiliencePipeline<EmailSendResult> BuildResiliencePipeline()
    {
        return new ResiliencePipelineBuilder<EmailSendResult>()
            // Retry policy: 3 попытки с exponential backoff
            .AddRetry(new RetryStrategyOptions<EmailSendResult>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<EmailSendResult>()
                    .HandleResult(result => !result.Success)
                    .Handle<SmtpException>()
                    .Handle<TimeoutException>()
                    .Handle<IOException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Email retry attempt {Attempt} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Result failure");
                    return ValueTask.CompletedTask;
                }
            })
            // Circuit breaker: break after 50% failure rate in 30s window
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<EmailSendResult>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<EmailSendResult>()
                    .HandleResult(result => !result.Success)
                    .Handle<SmtpException>()
                    .Handle<TimeoutException>(),
                OnOpened = args =>
                {
                    _logger.LogError(
                        "Circuit breaker opened! Email sending suspended for {Duration}s",
                        args.BreakDuration.TotalSeconds);
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("Circuit breaker closed. Email sending resumed.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation("Circuit breaker half-opened. Testing email connection...");
                    return ValueTask.CompletedTask;
                }
            })
            // Timeout: 45 seconds per email
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(45),
                OnTimeout = args =>
                {
                    _logger.LogWarning("Email sending timeout after 45s");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
