using Microsoft.AspNetCore.DataProtection;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NewsWave.NewsMaker;

public sealed class NewsMakerSettings
{
    public ProgramSettings Program { get; set; } = new();
    public BridgeNoteSettings BridgeNote { get; set; } = new();
    public PostSettings Post { get; set; } = new();
    public CampaignSettings Campaign { get; set; } = new();
    public int SendLimit { get; set; } = 10;
    public string ExportPath { get; set; } = "App_Data/exports";
}
public sealed class CampaignSettings
{
    public Guid? TemplateId { get; set; }
    [StringLength(200)] public string Subject { get; set; } = "Обновления на Bridgenote для #hello_name";
    public string Body { get; set; } = "";
    public bool IsHtml { get; set; } = true;
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
    private const string ProtectedPrefix = "protected:";
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly IDataProtector _protector;
    private readonly ILogger<NewsMakerSettingsStore> _logger;
    private readonly string _path;
    private bool _needsSecretMigration;
    private NewsMakerSettings _settings;

    public NewsMakerSettingsStore(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IDataProtectionProvider protectionProvider,
        ILogger<NewsMakerSettingsStore> logger)
    {
        _path = Path.Combine(environment.ContentRootPath, "App_Data", "newsmaker-settings.json");
        _protector = protectionProvider.CreateProtector("NewsWave.Settings.Secrets.v1");
        _logger = logger;
        _settings = Load(configuration);
        if (_needsSecretMigration)
            MigrateSecrets();
    }

    public NewsMakerSettings Current => Clone(_settings);

    public async Task SaveAsync(NewsMakerSettings settings, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            _settings = Clone(settings);
            NewsMakerSettings storage = Clone(_settings);
            storage.BridgeNote.Password = ProtectSecret(storage.BridgeNote.Password);
            storage.Post.PassWrd = ProtectSecret(storage.Post.PassWrd);

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            string temporaryPath = _path + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, JsonSerializer.Serialize(storage, _json), cancellationToken);
            File.Move(temporaryPath, _path, true);
        }
        finally { _gate.Release(); }
    }

    private NewsMakerSettings Load(IConfiguration configuration)
    {
        NewsMakerSettings settings = configuration.GetSection("NewsMaker").Get<NewsMakerSettings>() ?? new();
        if (File.Exists(_path))
        {
            try
            {
                settings = JsonSerializer.Deserialize<NewsMakerSettings>(File.ReadAllText(_path), _json) ?? settings;
                _needsSecretMigration =
                    IsUnprotected(settings.BridgeNote.Password) ||
                    IsUnprotected(settings.Post.PassWrd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось прочитать настройки NewsWave.");
            }
        }

        settings.BridgeNote.Password = UnprotectSecret(settings.BridgeNote.Password, "BridgeNote");
        settings.Post.PassWrd = UnprotectSecret(settings.Post.PassWrd, "SMTP");
        return settings;
    }

    private static bool IsUnprotected(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !value.StartsWith(ProtectedPrefix, StringComparison.Ordinal);

    private void MigrateSecrets()
    {
        try
        {
            NewsMakerSettings storage = Clone(_settings);
            storage.BridgeNote.Password = ProtectSecret(storage.BridgeNote.Password);
            storage.Post.PassWrd = ProtectSecret(storage.Post.PassWrd);
            string temporaryPath = _path + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(storage, _json));
            File.Move(temporaryPath, _path, true);
            _needsSecretMigration = false;
            _logger.LogInformation("Секреты NewsWave перенесены в защищённое хранилище.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось перенести секреты NewsWave в защищённое хранилище.");
        }
    }

    private string? ProtectSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
            return value;
        return ProtectedPrefix + _protector.Protect(value);
    }

    private string? UnprotectSecret(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(ProtectedPrefix, StringComparison.Ordinal))
            return value;
        try { return _protector.Unprotect(value[ProtectedPrefix.Length..]); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось расшифровать секрет {SecretName}.", name);
            return string.Empty;
        }
    }

    private NewsMakerSettings Clone(NewsMakerSettings value) =>
        JsonSerializer.Deserialize<NewsMakerSettings>(JsonSerializer.Serialize(value, _json), _json) ?? new();
}
