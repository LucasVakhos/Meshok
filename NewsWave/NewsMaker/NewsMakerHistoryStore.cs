using System.Text.Json;

namespace NewsWave.NewsMaker;

public sealed class NewsMakerHistoryStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly ILogger<NewsMakerHistoryStore> _logger;
    private readonly string _path;
    private List<HistoryEntry> _entries;

    public NewsMakerHistoryStore(IWebHostEnvironment environment, ILogger<NewsMakerHistoryStore> logger)
    {
        _logger = logger;
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "run-history.json");
        _entries = Load();
    }

    public NewsMakerSnapshot? Latest
    {
        get
        {
            lock (_sync)
                return _entries.OrderByDescending(x => x.StartedAt).FirstOrDefault()?.ToSnapshot();
        }
    }

    public IReadOnlyList<NewsMakerSnapshot> GetRecent(int count = 20)
    {
        lock (_sync)
            return _entries.OrderByDescending(x => x.StartedAt)
                .Take(Math.Max(1, count))
                .Select(x => x.ToSnapshot())
                .ToArray();
    }

    public async Task AppendAsync(NewsMakerSnapshot snapshot, CancellationToken token)
    {
        if (snapshot.StartedAt is null || snapshot.Status is not
            (NewsMakerRunStatus.Completed or NewsMakerRunStatus.Stopped or NewsMakerRunStatus.Failed))
            return;

        await _gate.WaitAsync(token);
        try
        {
            HistoryEntry entry = HistoryEntry.From(snapshot);
            string json;
            lock (_sync)
            {
                _entries.RemoveAll(x => x.StartedAt == entry.StartedAt);
                _entries.Add(entry);
                _entries = _entries.OrderByDescending(x => x.StartedAt).Take(100).ToList();
                json = JsonSerializer.Serialize(_entries, _json);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            string temporaryPath = _path + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, json, token);
            File.Move(temporaryPath, _path, true);
        }
        finally { _gate.Release(); }
    }

    private List<HistoryEntry> Load()
    {
        if (!File.Exists(_path))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(_path), _json) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось прочитать историю запусков NewsWave.");
            return [];
        }
    }

    private sealed record HistoryEntry(
        NewsMakerRunStatus Status,
        string Message,
        int Current,
        int Total,
        DateTimeOffset StartedAt,
        DateTimeOffset? CompletedAt,
        string[] Report)
    {
        public static HistoryEntry From(NewsMakerSnapshot value) =>
            new(value.Status, value.Message, value.Current, value.Total, value.StartedAt!.Value, value.CompletedAt, value.Report.ToArray());

        public NewsMakerSnapshot ToSnapshot() =>
            new(Status, Message, Current, Total, StartedAt, CompletedAt, Report);
    }
}
