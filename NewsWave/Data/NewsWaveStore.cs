using System.Text.Json;

namespace NewsWave.Data;

public sealed record ContactRecord(Guid Id, string Name, string Email, string Group, bool IsActive, DateTimeOffset CreatedAt);

public sealed record MailTemplateRecord(Guid Id, string Name, string Subject, string Body, bool IsHtml, DateTimeOffset UpdatedAt);

public sealed class NewsWaveStore
{
    private readonly object _sync = new();
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly ILogger<NewsWaveStore> _logger;
    private readonly string _filePath;
    private NewsWaveData _data;

    public NewsWaveStore(IWebHostEnvironment environment, ILogger<NewsWaveStore> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(environment.ContentRootPath, "App_Data", "newswave-data.json");
        _data = Load();
    }

    public IReadOnlyList<ContactRecord> GetContacts()
    {
        lock (_sync)
            return _data.Contacts.OrderByDescending(x => x.IsActive).ThenBy(x => x.Group).ThenBy(x => x.Name).ToArray();
    }

    public IReadOnlyList<MailTemplateRecord> GetTemplates()
    {
        lock (_sync)
            return _data.Templates.OrderByDescending(x => x.UpdatedAt).ToArray();
    }

    public MailTemplateRecord? FindTemplate(Guid id)
    {
        lock (_sync)
            return _data.Templates.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddContactAsync(string name, string email, string? group, bool isActive, CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            lock (_sync)
            {
                if (_data.Contacts.Any(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Такой email уже есть в адресной книге.");

                _data.Contacts.Add(new ContactRecord(Guid.NewGuid(), name.Trim(), email.Trim(), group?.Trim() ?? string.Empty, isActive, DateTimeOffset.Now));
            }

            await SaveAsync(cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task DeleteContactAsync(Guid id, CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            lock (_sync)
                _data.Contacts.RemoveAll(x => x.Id == id);
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task AddTemplateAsync(string name, string subject, string body, bool isHtml, CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            lock (_sync)
                _data.Templates.Add(new MailTemplateRecord(Guid.NewGuid(), name.Trim(), subject.Trim(), body, isHtml, DateTimeOffset.Now));
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task DeleteTemplateAsync(Guid id, CancellationToken cancellationToken)
    {
        await _writeGate.WaitAsync(cancellationToken);
        try
        {
            lock (_sync)
                _data.Templates.RemoveAll(x => x.Id == id);
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private NewsWaveData Load()
    {
        if (!File.Exists(_filePath))
            return new NewsWaveData();

        try
        {
            return JsonSerializer.Deserialize<NewsWaveData>(File.ReadAllText(_filePath), _json) ?? new NewsWaveData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось прочитать хранилище NewsWave.");
            return new NewsWaveData();
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        string json;
        lock (_sync)
            json = JsonSerializer.Serialize(_data, _json);

        string directory = Path.GetDirectoryName(_filePath) ?? throw new InvalidOperationException("Не найден каталог данных.");
        Directory.CreateDirectory(directory);
        string temporaryPath = _filePath + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
        File.Move(temporaryPath, _filePath, true);
    }

    private sealed class NewsWaveData
    {
        public List<ContactRecord> Contacts { get; set; } = [];
        public List<MailTemplateRecord> Templates { get; set; } = [];
    }
}
