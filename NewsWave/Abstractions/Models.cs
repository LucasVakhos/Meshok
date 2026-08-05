namespace NewsWave.Abstractions;

/// <summary>
/// Подписчик на рассылку
/// </summary>
public sealed record Subscriber(
    int Id,
    string Email,
    string? Name = null,
    DateTime? SubscribedAt = null,
    bool IsActive = true);

/// <summary>
/// Получатель письма с персонализацией
/// </summary>
public sealed record Recipient(
    int Id,
    string Name,
    string Email,
    string UnsubscribeUrl,
    string IdempotencyKey);

/// <summary>
/// Новость из буфера
/// </summary>
public sealed record NewsItem(
    int Id,
    string Title,
    string Content,
    DateTime PublishedAt,
    string? ImageUrl = null);

/// <summary>
/// Интервал отправки писем
/// </summary>
public sealed record SendInterval(
    DateTime Begin,
    DateTime End);
