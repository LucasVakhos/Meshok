using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NewsWave.NewsMaker;

public sealed class NewsMakerSettings
{
    public ProgramSettings Program { get; set; } = new();
    public BridgeNoteSettings BridgeNote { get; set; } = new();
    public PostSettings Post { get; set; } = new();
    public int SendLimit { get; set; } = 10;
    public string ExportPath { get; set; } = "App_Data/exports";
}
public sealed class ProgramSettings
{    [Range(0, 7)] public int RunDay { get; set; } = 7;
    public TimeSpan RunTime { get; set; } = TimeSpan.FromHours(18);
}
public sealed class BridgeNoteSettings
{
    public string? Server { get; set; }
    public string? Database { get; set; }
    public string? UserID { get; set; }
    public string? Password { get; set; }
    [Range(1, 65535)] public int Port { get; set; } = 3306;
    public string CharacterSet { get; set; } = "utf8";
    public string ConnectionProtocol { get; set; } = "Tcp";
    public string SslMode { get; set; } = "None";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Database) && !string.IsNullOrWhiteSpace(UserID);
}
public sealed class PostSettings
{
    public string? Smtp { get; set; }
    public string? User { get; set; }
    public string? PassWrd { get; set; }
    [EmailAddress] public string? BridgeEmail { get; set; }
    public string? ContactPhone { get; set; }
    [Range(1, 65535)] public int Port { get; set; } = 25;
    public bool UseSSL { get; set; } = true;
    [EmailAddress] public string? DeveloperEmail { get; set; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Smtp) && !string.IsNullOrWhiteSpace(BridgeEmail) && Port is > 0 and <= 65535;
}
public sealed class NewsMakerSettingsStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _path;
    private NewsMakerSettings _settings;
    public NewsMakerSettingsStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "newsmaker-settings.json");
        _settings = Load(configuration);
    }
    public NewsMakerSettings Current => Clone(_settings);
    public async Task SaveAsync(NewsMakerSettings settings, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _settings = Clone(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(_settings, _json), cancellationToken);
        }
        finally { _gate.Release(); }
    }
    private NewsMakerSettings Load(IConfiguration configuration)
    {
        if (File.Exists(_path))
        {
            try { return JsonSerializer.Deserialize<NewsMakerSettings>(File.ReadAllText(_path), _json) ?? new(); }
            catch { }
        }
        return configuration.GetSection("NewsMaker").Get<NewsMakerSettings>() ?? new();
    }
    private NewsMakerSettings Clone(NewsMakerSettings value) =>
        JsonSerializer.Deserialize<NewsMakerSettings>(JsonSerializer.Serialize(value, _json), _json) ?? new();
}
