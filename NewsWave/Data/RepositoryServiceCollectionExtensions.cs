using NewsWave.Abstractions;
using NewsWave.Data.Repositories;

namespace NewsWave.Data;

/// <summary>
/// Регистрация репозиториев в DI
/// </summary>
public static class RepositoryServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Dapper-based репозитории
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Регистрация репозиториев как Scoped для безопасной работы с БД
        // Note: Polly resilience встроен в Dapper через retry на уровне MySQL driver
        services.AddScoped<ISubscriberRepository, SubscriberRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();

        return services;
    }
}
