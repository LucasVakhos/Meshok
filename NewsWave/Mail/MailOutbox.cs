using Microsoft.Extensions.Options;
using NewsWave.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Channels;

namespace NewsWave.Mail;

public interface IMailQueue
{
    Task<Guid> EnqueueAsync(MailRequest request, CancellationToken cancellationToken = default);
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
        Channel.CreateUnbounded<MailDispatch>(new UnboundedChannelOptions { SingleReader = true });
    private readonly IOptionsMonitor<MailOptions> _options;
    private readonly NewsWaveStore _store;
    private readonly ILogger<MailOutbox> _logger;

    public MailOutbox(
        IOptionsMonitor<MailOptions> options,
        NewsWaveStore store,
        ILogger<MailOutbox> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<Guid> EnqueueAsync(MailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        MailDispatch dispatch = new(request);
        await _store.UpsertDispatchAsync(dispatch.Snapshot(), cancellationToken);
        if (!_queue.Writer.TryWrite(dispatch))
            throw new InvalidOperationException("Очередь почты недоступна.");

        return dispatch.Id;
    }

    public IReadOnlyList<MailDispatchSnapshot> Recent(int count = 20) =>
        _store.GetDispatches(count);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (MailDispatch dispatch in _queue.Reader.ReadAllAsync(stoppingToken))
            await SendAsync(dispatch, stoppingToken);
    }

    private async Task SendAsync(MailDispatch dispatch, CancellationToken cancellationToken)
    {
        MailOptions options = _options.CurrentValue;
        if (!options.IsConfigured)
        {
            dispatch.Fail("SMTP не настроен.");
            await PersistSafeAsync(dispatch);
            return;
        }

        dispatch.Start();
        await PersistSafeAsync(dispatch);

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

            MailAddress sender = new(options.FromAddress, options.FromName, Encoding.UTF8);

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
            await PersistSafeAsync(dispatch);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            dispatch.Fail("Отправка остановлена вместе с приложением.");
            await PersistSafeAsync(dispatch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось выполнить рассылку {DispatchId}", dispatch.Id);
            dispatch.Fail(ex.Message);
            await PersistSafeAsync(dispatch);
        }
    }

    private async Task PersistSafeAsync(MailDispatch dispatch)
    {
        try
        {
            await _store.UpsertDispatchAsync(dispatch.Snapshot());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сохранить состояние рассылки {DispatchId}", dispatch.Id);
        }
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
