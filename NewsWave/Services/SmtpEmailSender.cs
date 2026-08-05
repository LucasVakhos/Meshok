using Microsoft.Extensions.Options;
using NewsWave.Abstractions;
using NewsWave.Configuration;
using System.Net;
using System.Net.Mail;

namespace NewsWave.Services;

/// <summary>
/// SMTP реализация отправки email с поддержкой resilience
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly NewsMakerOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<NewsMakerOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Post.IsConfigured)
        {
            const string error = "SMTP не настроен. Проверьте конфигурацию.";
            _logger.LogWarning(error);
            return new EmailSendResult(false, error);
        }

        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(message);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email отправлен успешно: {To}", message.To);
            return new EmailSendResult(true, SentAt: DateTime.UtcNow);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP ошибка при отправке email: {To}", message.To);
            return new EmailSendResult(false, $"SMTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке email: {To}", message.To);
            return new EmailSendResult(false, ex.Message);
        }
    }

    public async Task<IReadOnlyList<EmailSendResult>> SendBatchAsync(
        IEnumerable<EmailMessage> messages,
        CancellationToken cancellationToken = default)
    {
        var results = new List<EmailSendResult>();

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Batch отправка отменена");
                break;
            }

            var result = await SendAsync(message, cancellationToken);
            results.Add(result);

            // Небольшая задержка между отправками для предотвращения блокировки SMTP
            if (result.Success)
            {
                await Task.Delay(100, cancellationToken);
            }
        }

        return results;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Post.IsConfigured)
        {
            _logger.LogWarning("SMTP не настроен");
            return false;
        }

        try
        {
            using var smtpClient = CreateSmtpClient();

            // Проверка через отправку тестового письма разработчику
            if (!string.IsNullOrWhiteSpace(_options.Post.DeveloperEmail))
            {
                var testMessage = new EmailMessage(
                    To: _options.Post.DeveloperEmail,
                    Subject: "NewsWave SMTP Test",
                    Body: $"<p>Тестовое письмо от NewsWave</p><p>Время: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>",
                    IsHtml: true);

                using var mailMessage = CreateMailMessage(testMessage);
                await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }

            _logger.LogInformation("SMTP подключение успешно");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка SMTP подключения");
            return false;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var smtp = _options.Post;

        var client = new SmtpClient(smtp.Smtp, smtp.Port)
        {
            EnableSsl = smtp.UseSSL,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtp.User, smtp.PassWrd),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 30000 // 30 seconds
        };

        return client;
    }

    private MailMessage CreateMailMessage(EmailMessage message)
    {
        var smtp = _options.Post;

        var from = !string.IsNullOrWhiteSpace(message.FromEmail)
            ? new MailAddress(message.FromEmail, message.FromName ?? "NewsWave")
            : new MailAddress(smtp.BridgeEmail ?? smtp.User ?? "", "BridgeNote");

        var mail = new MailMessage
        {
            From = from,
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml,
            Priority = MailPriority.Normal
        };

        mail.To.Add(message.To);

        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            mail.ReplyToList.Add(message.ReplyTo);
        }

        // Добавление custom headers
        if (message.Headers != null)
        {
            foreach (var (key, value) in message.Headers)
            {
                mail.Headers.Add(key, value);
            }
        }

        return mail;
    }
}
