using System.Data;
using System.Threading.Channels;

namespace NewsWave.NewsMaker;

public enum NewsMakerRunStatus { Idle, Preparing, Sending, Completed, Stopped, Failed }

public sealed record NewsMakerSnapshot(
    NewsMakerRunStatus Status,
    string Message,
    int Current,
    int Total,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<string> Report)
{
    public bool Running => Status is NewsMakerRunStatus.Preparing or NewsMakerRunStatus.Sending;
}

public interface INewsMakerRunner
{
    NewsMakerSnapshot Snapshot();
    bool Start();
    void Stop();
    Task SendReportAsync(CancellationToken token);
}

public sealed class NewsMakerRunService : BackgroundService, INewsMakerRunner
{
    private readonly Channel<bool> _requests = Channel.CreateBounded<bool>(1);
    private readonly object _sync = new();
    private readonly BridgeNoteRepository _database;
    private readonly NewsletterArchiveBuilder _archiveBuilder;
    private readonly NewsletterSmtpSender _sender;
    private readonly NewsMakerSettingsStore _settings;
    private readonly ILogger<NewsMakerRunService> _logger;
    private CancellationTokenSource? _runCancellation;
    private NewsMakerRunStatus _status = NewsMakerRunStatus.Idle;
    private string _message = "Готов к работе";
    private int _current;
    private int _total;
    private DateTimeOffset? _started;
    private DateTimeOffset? _completed;
    private readonly List<string> _report = [];

    public NewsMakerRunService(
        BridgeNoteRepository database,
        NewsletterArchiveBuilder archiveBuilder,
        NewsletterSmtpSender sender,
        NewsMakerSettingsStore settings,
        ILogger<NewsMakerRunService> logger)
    {
        _database = database;
        _archiveBuilder = archiveBuilder;
        _sender = sender;
        _settings = settings;
        _logger = logger;
    }

    public bool Start()
    {
        lock (_sync)
        {
            if (_status is NewsMakerRunStatus.Preparing or NewsMakerRunStatus.Sending)
                return false;
            _status = NewsMakerRunStatus.Preparing;
            _message = "Запуск рассылки...";
            _current = 0;
            _total = 0;
            _started = DateTimeOffset.Now;
            _completed = null;
            _report.Clear();
        }
        if (_requests.Writer.TryWrite(true))
            return true;
        SetStatus(NewsMakerRunStatus.Failed, "Не удалось поставить запуск в очередь.");
        return false;
    }

    public void Stop()
    {
        _runCancellation?.Cancel();
        AddReport("Работа остановлена...");
    }

    public NewsMakerSnapshot Snapshot()
    {
        lock (_sync)
            return new(_status, _message, _current, _total, _started, _completed, _report.ToArray());
    }

    public async Task SendReportAsync(CancellationToken token)
    {
        string report;
        lock (_sync) report = string.Join(Environment.NewLine, _report);
        if (report.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length < 3)
            throw new InvalidOperationException("Нет информации для отправки.");
        await _sender.SendReportAsync(report, token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan automaticCheckDelay = TimeSpan.FromSeconds(5);
        while (!stoppingToken.IsCancellationRequested)
        {
            Task<bool> read = _requests.Reader.ReadAsync(stoppingToken).AsTask();
            Task delay = Task.Delay(automaticCheckDelay, stoppingToken);
            Task completed = await Task.WhenAny(read, delay);
            if (completed == read)
            {
                await read;
                await RunSafeAsync(stoppingToken);
            }
            else
            {
                await TryAutoStartAsync(stoppingToken);
                automaticCheckDelay = TimeSpan.FromMinutes(5);
            }
        }
    }

    private async Task TryAutoStartAsync(CancellationToken token)
    {
        try
        {
            if (await _database.BufferCountAsync(token) > 0)
            {
                Start();
                return;
            }

            SendInterval interval = await _database.ReadIntervalAsync(token);
            if (interval.Begin < interval.End)
            {
                Start();
                return;
            }

            NewsMakerSettings settings = _settings.Current;
            DateTime now = DateTime.Now;
            if (settings.Program.RunDay == 7 ||
                DateTime.Today == interval.End.Date ||
                (int)DateTime.Today.DayOfWeek != settings.Program.RunDay ||
                now.TimeOfDay < settings.Program.RunTime)
                return;

            Start();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Автоматическая проверка NewsMaker пропущена.");
        }
    }

    private async Task RunSafeAsync(CancellationToken stoppingToken)
    {
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _runCancellation = linked;
        try
        {
            await RunAsync(linked.Token);
        }
        catch (OperationCanceledException) when (linked.IsCancellationRequested)
        {
            SetStatus(NewsMakerRunStatus.Stopped, "Работа остановлена...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка конвейера NewsMaker.");
            AddReport(ex.Message);
            SetStatus(NewsMakerRunStatus.Failed, "Работа остановлена из-за ошибки.");
        }
        finally
        {
            _runCancellation = null;
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        AddReport("Рассылка обновлений Bridgenote.com");
        await _database.TestAsync(token);
        AddReport("Подключение к базе данных установлено.");

        SendInterval interval = await _database.ReadIntervalAsync(token);
        if (interval.Begin == interval.End)
        {
            interval = interval with { End = DateTime.Today };
            await _database.WriteIntervalAsync(interval, token);
        }

        AddReport("Проверяем список новинок...");
        if (await _database.CheckNewsAsync(token) <= 0)
        {
            AddReport("Новости отсутствуют, завершаем работу...");
            await ResetIntervalAsync(interval, token);
            SetStatus(NewsMakerRunStatus.Completed, "Списки для рассылки пусты.");
            return;
        }

        AddReport("Проверяем список подписчиков...");
        if (await _database.CheckSubscribersAsync(token) <= 0)
        {
            AddReport("Подписчиков нет или все отписались, завершаем работу...");
            await ResetIntervalAsync(interval, token);
            SetStatus(NewsMakerRunStatus.Completed, "Список подписки пуст.");
            return;
        }

        AddReport("Создание списка новинок...");
        using DataTable news = await _database.GetNewsAsync(token);
        if (news.Rows.Count == 0)
        {
            await _database.ClearBufferAsync(token);
            await ResetIntervalAsync(interval, token);
            SetStatus(NewsMakerRunStatus.Completed, "Список новинок пуст.");
            return;
        }
        AddReport($"Всего новинок {news.Rows.Count}");
        NewsletterArchive archive = _archiveBuilder.Build(news, interval.End);
        AddReport($"Создан {archive.FileName}");

        AddReport("Получаем список подписчиков...");
        await _database.PrepareSubscribersAsync(token);
        IReadOnlyList<BridgeRecipient> recipients = await _database.GetBufferAsync(token);
        SetProgress(NewsMakerRunStatus.Sending, "Рассылка сообщений...", 0, recipients.Count);

        int limit = Math.Max(1, _settings.Current.SendLimit);
        int sentInWindow = 0;
        DateTime window = DateTime.UtcNow;
        foreach (BridgeRecipient recipient in recipients)
        {
            token.ThrowIfCancellationRequested();
            if ((DateTime.UtcNow - window).TotalSeconds >= 1)
            {
                window = DateTime.UtcNow;
                sentInWindow = 0;
            }
            if (sentInWindow >= limit)
            {
                TimeSpan wait = TimeSpan.FromSeconds(1) - (DateTime.UtcNow - window);
                if (wait > TimeSpan.Zero)
                    await Task.Delay(wait, token);
                window = DateTime.UtcNow;
                sentInWindow = 0;
            }

            (string text, string html) = _archiveBuilder.Personalize(recipient.Name, recipient.UnsubscribeUrl);
            await _sender.SendNewsletterAsync(recipient, archive, text, html, token);
            await _database.DeleteBufferItemAsync(recipient.Id, token);
            sentInWindow++;
            SetProgress(NewsMakerRunStatus.Sending, "Рассылка сообщений...", _current + 1, recipients.Count);
            AddReport($"Отправлено: {recipient.Email}");
        }

        if (await _database.BufferCountAsync(token) == 0)
            await ResetIntervalAsync(interval, token);
        AddReport("Работа завершена...");
        SetStatus(NewsMakerRunStatus.Completed, "Работа завершена.");
    }

    private async Task ResetIntervalAsync(SendInterval interval, CancellationToken token)
    {
        await _database.WriteIntervalAsync(interval with { Begin = interval.End }, token);
    }

    private void AddReport(string text)
    {
        lock (_sync)
        {
            _message = text;
            _report.Add($"{DateTime.Now:HH:mm:ss}  {text}");
            if (_report.Count > 1000)
                _report.RemoveAt(0);
        }
    }

    private void SetProgress(NewsMakerRunStatus status, string message, int current, int total)
    {
        lock (_sync)
        {
            _status = status;
            _message = message;
            _current = current;
            _total = total;
        }
    }

    private void SetStatus(NewsMakerRunStatus status, string message)
    {
        lock (_sync)
        {
            _status = status;
            _message = message;
            if (status is NewsMakerRunStatus.Completed or NewsMakerRunStatus.Stopped or NewsMakerRunStatus.Failed)
                _completed = DateTimeOffset.Now;
        }
    }
}
