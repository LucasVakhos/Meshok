using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Channels;

namespace NewsWave.Mail;

public interface IMailQueue
{
    Guid Enqueue(MailRequest request);
    IReadOnlyList<MailDispatchSnapshot> Recent(int count = 20);
}

public enum MailDispatchStatus
{
    Queued,
    Sending,
    Sent,
    Failed
}

public sealed record MailDispatchSnapshot(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    string Subject,
    int RecipientCount,
    int SentCount,
    MailDispatchStatus Status,
    string? Error);

public sealed class MailOutbox : BackgroundService, IMailQueue
{
    private readonly Channel<MailDispatch> _queue =
        Channel.CreateUnbounded<MailDispatch>(
            new UnboundedChannelOptions { SingleReader = true });
    private readonly ConcurrentDictionary<Guid, MailDispatch> _dispatches = new();
    private readonly IOptionsMonitor<MailOptions> _options;
    private readonly ILogger<MailOutbox> _logger;

    public MailOutbox(
        IOptionsMonitor<MailOptions> options,
        ILogger<MailOutbox> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Guid Enqueue(MailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        MailDispatch dispatch = new(request);
        _dispatches[dispatch.Id] = dispatch;
        if (!_queue.Writer.TryWrite(dispatch))
            throw new InvalidOperationException("Очередь почты недоступна.");

        TrimHistory();
        return dispatch.Id;
    }

    public IReadOnlyList<MailDispatchSnapshot> Recent(int count = 20)
    {
        return _dispatches.Values
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(count, 1, 100))
            .Select(x => x.Snapshot())
            .ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (MailDispatch dispatch in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await SendAsync(dispatch, stoppingToken);
        }
    }

    private async Task SendAsync(
        MailDispatch dispatch,
        CancellationToken cancellationToken)
    {
        MailOptions options = _options.CurrentValue;
        if (!options.IsConfigured)
        {
            dispatch.Fail("SMTP не настроен.");
            return;
        }

        dispatch.Start();
        try
        {
            using SmtpClient client = new(options.Host, options.Port)
            {
                EnableSsl = options.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(options.UserName))
                client.Credentials = new NetworkCredential(options.UserName, options.Password);

            MailAddress sender = new(
                options.FromAddress,
                options.FromName,
                Encoding.UTF8);

            foreach (string recipient in dispatch.Request.Recipients)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using MailMessage message = new()
                {
                    From = sender,
                    Subject = dispatch.Request.Subject,
                    Body = dispatch.Request.Body,
                    IsBodyHtml = dispatch.Request.IsHtml,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };
                message.To.Add(recipient);

                await client.SendMailAsync(message, cancellationToken);
                dispatch.MarkRecipientSent();
            }

            dispatch.Complete();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            dispatch.Fail("Отправка остановлена вместе с приложением.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось выполнить рассылку {DispatchId}", dispatch.Id);
            dispatch.Fail(ex.Message);
        }
    }

    private void TrimHistory()
    {
        MailDispatch[] overflow = _dispatches.Values
            .OrderByDescending(x => x.CreatedAt)
            .Skip(100)
            .ToArray();

        foreach (MailDispatch dispatch in overflow)
            _dispatches.TryRemove(dispatch.Id, out _);
    }

    private sealed class MailDispatch
    {
        private readonly object _sync = new();
        private int _sentCount;
        private MailDispatchStatus _status = MailDispatchStatus.Queued;
        private DateTimeOffset? _completedAt;
        private string? _error;

        public MailDispatch(MailRequest request)
        {
            Request = request;
        }

        public Guid Id { get; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
        public MailRequest Request { get; }

        public void Start()
        {
            lock (_sync)
                _status = MailDispatchStatus.Sending;
        }

        public void MarkRecipientSent()
        {
            lock (_sync)
                _sentCount++;
        }

        public void Complete()
        {
            lock (_sync)
            {
                _status = MailDispatchStatus.Sent;
                _completedAt = DateTimeOffset.Now;
            }
        }

        public void Fail(string error)
        {
            lock (_sync)
            {
                _status = MailDispatchStatus.Failed;
                _error = error;
                _completedAt = DateTimeOffset.Now;
            }
        }

        public MailDispatchSnapshot Snapshot()
        {
            lock (_sync)
            {
                return new MailDispatchSnapshot(
                    Id,
                    CreatedAt,
                    _completedAt,
                    Request.Subject,
                    Request.Recipients.Count,
                    _sentCount,
                    _status,
                    _error);
            }
        }
    }
}
