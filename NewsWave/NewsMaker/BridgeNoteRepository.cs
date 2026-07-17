using MySql.Data.MySqlClient;
using System.Data;

namespace NewsWave.NewsMaker;

public sealed record BridgeRecipient(int Id, string Name, string Email, string UnsubscribeUrl, string IdempotencyKey);
public sealed record SendInterval(DateTime Begin, DateTime End);

public sealed class BridgeNoteRepository
{
    private readonly NewsMakerSettingsStore _settings;
    public BridgeNoteRepository(NewsMakerSettingsStore settings) => _settings = settings;

    public async Task TestAsync(CancellationToken token)
    {
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
    }

    public async Task<IReadOnlyList<string>> GetEmailsAsync(CancellationToken token)
    {
        const string sql = "SELECT email FROM subscribers WHERE email IS NOT NULL AND TRIM(email) <> '' ORDER BY email";
        List<string> emails = [];
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
            emails.Add(reader.GetString(0).Trim());
        return emails.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task AddEmailAsync(string email, CancellationToken token)
    {
        string normalized = email.Trim();
        if (await EmailExistsAsync(normalized, token))
            throw new InvalidOperationException("Такой email уже есть в subscribers.");

        const string sql = "INSERT INTO subscribers (email) VALUES (@email)";
        await ExecuteAsync(sql, token, ("@email", normalized));
    }

    public Task DeleteEmailAsync(string email, CancellationToken token) =>
        ExecuteAsync("DELETE FROM subscribers WHERE LOWER(TRIM(email)) = LOWER(@email)",
            token, ("@email", email.Trim()));

    private async Task<bool> EmailExistsAsync(string email, CancellationToken token)
    {
        const string sql = "SELECT COUNT(*) FROM subscribers WHERE LOWER(TRIM(email)) = LOWER(@email)";
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@email", email);
        return Convert.ToInt32(await command.ExecuteScalarAsync(token)) > 0;
    }

    public Task<int> CheckNewsAsync(CancellationToken token) => ScalarProcedureAsync("chk_news", token);
    public Task<int> CheckSubscribersAsync(CancellationToken token) => ScalarProcedureAsync("chk_subscribers", token);

    public async Task PrepareSubscribersAsync(CancellationToken token)
    {
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new("chk_subscribers", connection) { CommandType = CommandType.StoredProcedure };
        await command.ExecuteNonQueryAsync(token);
    }

    public async Task<DataTable> GetNewsAsync(CancellationToken token)
    {
        DataTable table = new("News Bridgenote");
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new("SELECT * FROM new_prix_list", connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        table.Load(reader);
        return table;
    }

    public async Task<IReadOnlyList<BridgeRecipient>> GetBufferAsync(CancellationToken token)
    {
        const string sql = "SELECT id, name, email, unsubscribe_url, unique_key AS idempotencyKey FROM v_ss_buffer";
        List<BridgeRecipient> recipients = [];
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
        {
            recipients.Add(new(
                Convert.ToInt32(reader["id"]),
                Convert.ToString(reader["name"])?.Trim() ?? string.Empty,
                Convert.ToString(reader["email"])?.Trim() ?? string.Empty,
                Convert.ToString(reader["unsubscribe_url"])?.Trim() ?? string.Empty,
                Convert.ToString(reader["idempotencyKey"])?.Trim() ?? string.Empty));
        }
        return recipients;
    }

    public async Task<SendInterval> ReadIntervalAsync(CancellationToken token)
    {
        const string sql = "SELECT sss_upd_interval_begin, sss_upd_interval_end FROM subscribers_send_setting WHERE sss_id = 1";
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token))
            throw new InvalidOperationException("Не найдена строка sss_id = 1 в subscribers_send_setting.");
        return new(reader.GetDateTime(0), reader.GetDateTime(1));
    }

    public Task WriteIntervalAsync(SendInterval value, CancellationToken token) =>
        ExecuteAsync("UPDATE subscribers_send_setting SET sss_upd_interval_begin=@begin, sss_upd_interval_end=@end WHERE sss_id=1",
            token, ("@begin", value.Begin), ("@end", value.End));

    public Task DeleteBufferItemAsync(int id, CancellationToken token) =>
        ExecuteAsync("DELETE FROM subscribers_send_buffer WHERE id=@id", token, ("@id", id));

    public Task ClearBufferAsync(CancellationToken token) =>
        ExecuteAsync("DELETE FROM subscribers_send_buffer", token);

    public async Task<int> BufferCountAsync(CancellationToken token)
    {
        const string sql = "SELECT COUNT(sb.id) FROM subscribers_send_buffer sb INNER JOIN subscribers_send_setting sss ON (sss.sss_id=1) WHERE sb.date_sending=sss.sss_upd_interval_end";
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(token));
    }

    private async Task<int> ScalarProcedureAsync(string name, CancellationToken token)
    {
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlCommand command = new(name, connection) { CommandType = CommandType.StoredProcedure };
        return Convert.ToInt32(await command.ExecuteScalarAsync(token));
    }

    private async Task ExecuteAsync(string sql, CancellationToken token, params (string Name, object Value)[] values)
    {
        await using MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(token);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(token);
        await using MySqlCommand command = new(sql, connection, transaction);
        foreach ((string name, object value) in values)
            command.Parameters.AddWithValue(name, value);
        await command.ExecuteNonQueryAsync(token);
        await transaction.CommitAsync(token);
    }

    private MySqlConnection CreateConnection()
    {
        BridgeNoteSettings value = _settings.Current.BridgeNote;
        if (!value.IsConfigured)
            throw new InvalidOperationException("Подключение BridgeNote не настроено.");
        MySqlConnectionStringBuilder builder = new()
        {
            Server = value.Server ?? string.Empty,
            Database = value.Database ?? string.Empty,
            UserID = value.UserID ?? string.Empty,
            Password = value.Password ?? string.Empty,
            Port = (uint)value.Port,
            CharacterSet = value.CharacterSet,
            Pooling = true
        };
        if (Enum.TryParse(value.SslMode, true, out MySqlSslMode ssl))
            builder.SslMode = ssl;
        return new(builder.ConnectionString);
    }
}
