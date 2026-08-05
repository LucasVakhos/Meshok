using Microsoft.Extensions.Configuration;

namespace MehokBrowser.Services;

/// <summary>
/// Сервис для type-safe доступа к настройкам приложения
/// </summary>
public interface IConfigurationService
{
    string ApplicationName { get; }
    string ApplicationVersion { get; }
    T GetValue<T>(string key, T defaultValue);
    T GetSection<T>(string sectionName) where T : class, new();
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string ApplicationName => _configuration["Application:Name"] ?? "MehokBrowser";

    public string ApplicationVersion => _configuration["Application:Version"] ?? "1.0.0";

    public T GetValue<T>(string key, T defaultValue)
    {
        return _configuration.GetValue<T>(key, defaultValue);
    }

    public T GetSection<T>(string sectionName) where T : class, new()
    {
        var section = new T();
        _configuration.GetSection(sectionName).Bind(section);
        return section;
    }
}
