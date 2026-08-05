namespace NewsWave.Abstractions;

/// <summary>
/// Репозиторий для работы с подписчиками и новостями
/// </summary>
public interface ISubscriberRepository
{
    /// <summary>
    /// Тест подключения к базе данных
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все email адреса подписчиков
    /// </summary>
    Task<IReadOnlyList<string>> GetEmailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить email подписчика
    /// </summary>
    Task AddEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить email подписчика
    /// </summary>
    Task DeleteEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, существует ли email
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Репозиторий для работы с новостями
/// </summary>
public interface INewsRepository
{
    /// <summary>
    /// Проверить наличие новых новостей
    /// </summary>
    Task<int> CheckNewsCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить новости за период
    /// </summary>
    Task<IReadOnlyList<NewsItem>> GetNewsAsync(SendInterval interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить данные из буфера новостей
    /// </summary>
    Task<IReadOnlyList<NewsItem>> GetBufferAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Репозиторий для работы с получателями рассылки
/// </summary>
public interface IRecipientRepository
{
    /// <summary>
    /// Проверить количество активных подписчиков
    /// </summary>
    Task<int> CheckSubscribersCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Подготовить получателей для рассылки
    /// </summary>
    Task PrepareRecipientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текущий интервал рассылки
    /// </summary>
    Task<SendInterval> GetSendIntervalAsync(CancellationToken cancellationToken = default);
}
