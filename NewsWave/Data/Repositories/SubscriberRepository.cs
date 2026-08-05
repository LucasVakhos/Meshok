using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NewsWave.Abstractions;
using NewsWave.Configuration;
using System.Data;

namespace NewsWave.Data.Repositories;

/// <summary>
/// Dapper-based реализация репозитория подписчиков
/// </summary>
public sealed class SubscriberRepository : ISubscriberRepository
{
    private readonly NewsMakerOptions _options;
    private readonly ILogger<SubscriberRepository> _logger;

    public SubscriberRepository(
        IOptions<NewsMakerOptions> options,
        ILogger<SubscriberRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка подключения к базе данных BridgeNote");
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetEmailsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT email 
            FROM subscribers 
            WHERE email IS NOT NULL 
              AND TRIM(email) <> '' 
            ORDER BY email";

        await using var connection = CreateConnection();
        var emails = await connection.QueryAsync<string>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return emails
            .Select(e => e.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task AddEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalized = email.Trim();

        if (await EmailExistsAsync(normalized, cancellationToken))
        {
            throw new InvalidOperationException($"Email '{normalized}' уже существует в базе подписчиков.");
        }

        const string sql = "INSERT INTO subscribers (email) VALUES (@Email)";

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Email = normalized },
                cancellationToken: cancellationToken));

        _logger.LogInformation("Добавлен новый подписчик: {Email}", normalized);
    }

    public async Task DeleteEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM subscribers WHERE LOWER(TRIM(email)) = LOWER(@Email)";

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Email = email.Trim() },
                cancellationToken: cancellationToken));

        if (affected > 0)
        {
            _logger.LogInformation("Удален подписчик: {Email}", email);
        }
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(*) FROM subscribers WHERE LOWER(TRIM(email)) = LOWER(@Email)";

        await using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { Email = email.Trim() },
                cancellationToken: cancellationToken));

        return count > 0;
    }

    private MySqlConnection CreateConnection()
    {
        if (!_options.BridgeNote.IsConfigured)
        {
            throw new InvalidOperationException(
                "BridgeNote database connection is not configured. " +
                "Please check your appsettings.json or user secrets.");
        }

        return new MySqlConnection(_options.BridgeNote.ConnectionString);
    }
}
