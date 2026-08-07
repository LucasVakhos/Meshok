using Microsoft.Extensions.Configuration;

namespace MehokBrowser.Services;

/// <summary>
/// Сервис для получения connection strings из конфигурации
/// </summary>
public interface IConnectionStringProvider
{
    string GetFirebirdConnectionString();
    string GetMySqlConnectionString();
}

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public ConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string GetFirebirdConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("Firebird");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Firebird connection string is not configured in appsettings.json");
        }
        return connectionString;
    }

    public string GetMySqlConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("MySQL");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("MySQL connection string is not configured in appsettings.json");
        }
        return connectionString;
    }
}
