namespace NewsWave.Abstractions;

/// <summary>
/// Email сообщение для отправки
/// </summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? FromEmail = null,
    string? FromName = null,
    string? ReplyTo = null,
    IDictionary<string, string>? Headers = null);

/// <summary>
/// Результат отправки email
/// </summary>
public sealed record EmailSendResult(
    bool Success,
    string? Error = null,
    DateTime? SentAt = null);

/// <summary>
/// Интерфейс отправки email
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Отправить email асинхронно
    /// </summary>
    Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить batch emails
    /// </summary>
    Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить настройки SMTP
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
