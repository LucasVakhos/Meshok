using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NewsWave.Abstractions;
using NewsWave.Configuration;
using System.Data;

namespace NewsWave.Data.Repositories;

/// <summary>
/// Dapper-based реализация репозитория новостей
/// </summary>
public sealed class NewsRepository : INewsRepository
{
    private readonly NewsMakerOptions _options;
    private readonly ILogger<NewsRepository> _logger;

    public NewsRepository(
        IOptions<NewsMakerOptions> options,
        ILogger<NewsRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> CheckNewsCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Вызов хранимой процедуры chk_news
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "chk_news",
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке количества новостей");
            return 0;
        }
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsAsync(
        SendInterval interval,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                title AS Title,
                content AS Content,
                published_at AS PublishedAt,
                image_url AS ImageUrl
            FROM new_prix_list
            WHERE published_at BETWEEN @Begin AND @End
            ORDER BY published_at DESC";

        await using var connection = CreateConnection();
        var news = await connection.QueryAsync<NewsItem>(
            new CommandDefinition(
                sql,
                new { Begin = interval.Begin, End = interval.End },
                cancellationToken: cancellationToken));

        return news.ToArray();
    }

    public async Task<IReadOnlyList<NewsItem>> GetBufferAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                title AS Title,
                content AS Content,
                date_sending AS PublishedAt,
                NULL AS ImageUrl
            FROM subscribers_send_buffer
            ORDER BY date_sending DESC";

        await using var connection = CreateConnection();
        var news = await connection.QueryAsync<NewsItem>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return news.ToArray();
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
