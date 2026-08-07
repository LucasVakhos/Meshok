using Dapper;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using NewsWave.Abstractions;
using NewsWave.Configuration;
using System.Data;

namespace NewsWave.Data.Repositories;

/// <summary>
/// Dapper-based реализация репозитория получателей рассылки
/// </summary>
public sealed class RecipientRepository : IRecipientRepository
{
    private readonly NewsMakerOptions _options;
    private readonly ILogger<RecipientRepository> _logger;

    public RecipientRepository(
        IOptions<NewsMakerOptions> options,
        ILogger<RecipientRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> CheckSubscribersCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Вызов хранимой процедуры chk_subscribers
            var count = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "chk_subscribers",
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке количества подписчиков");
            return 0;
        }
    }

    public async Task PrepareRecipientsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Вызов хранимой процедуры для подготовки получателей
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "chk_subscribers",
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));

            _logger.LogInformation("Получатели рассылки подготовлены успешно");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подготовке получателей рассылки");
            throw;
        }
    }

    public async Task<SendInterval> GetSendIntervalAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                sss_upd_interval_begin AS Begin,
                sss_upd_interval_end AS End
            FROM subscribers_send_setting 
            WHERE sss_id = 1";

        await using var connection = CreateConnection();
        var interval = await connection.QuerySingleOrDefaultAsync<SendInterval>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        if (interval == null)
        {
            throw new InvalidOperationException(
                "Не найдена строка sss_id = 1 в subscribers_send_setting. " +
                "Убедитесь, что база данных правильно инициализирована.");
        }

        return interval;
    }

    /// <summary>
    /// Получить список получателей из буфера рассылки
    /// </summary>
    public async Task<IReadOnlyList<Recipient>> GetRecipientsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                id AS Id,
                name AS Name,
                email AS Email,
                unsubscribe_url AS UnsubscribeUrl,
                unique_key AS IdempotencyKey
            FROM v_ss_buffer";

        await using var connection = CreateConnection();
        var recipients = await connection.QueryAsync<Recipient>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return recipients.ToArray();
    }

    /// <summary>
    /// Обновить интервал отправки
    /// </summary>
    public async Task UpdateSendIntervalAsync(
        SendInterval interval,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE subscribers_send_setting 
            SET 
                sss_upd_interval_begin = @Begin,
                sss_upd_interval_end = @End
            WHERE sss_id = 1";

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { interval.Begin, interval.End },
                cancellationToken: cancellationToken));

        _logger.LogInformation(
            "Интервал отправки обновлен: {Begin} - {End}",
            interval.Begin,
            interval.End);
    }

    /// <summary>
    /// Удалить элемент из буфера рассылки
    /// </summary>
    public async Task DeleteBufferItemAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM subscribers_send_buffer WHERE id = @Id";

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Очистить весь буфер рассылки
    /// </summary>
    public async Task ClearBufferAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM subscribers_send_buffer";

        await using var connection = CreateConnection();
        var affected = await connection.ExecuteAsync(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        _logger.LogInformation("Буфер рассылки очищен. Удалено записей: {Count}", affected);
    }

    /// <summary>
    /// Получить количество элементов в буфере
    /// </summary>
    public async Task<int> GetBufferCountAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(sb.id)
            FROM subscribers_send_buffer sb
            INNER JOIN subscribers_send_setting sss ON sss.sss_id = 1
            WHERE sb.date_sending = sss.sss_upd_interval_end";

        await using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
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
