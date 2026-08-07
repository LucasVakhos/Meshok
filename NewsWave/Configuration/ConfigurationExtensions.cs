using FluentValidation;
using Microsoft.Extensions.Options;

namespace NewsWave.Configuration;

/// <summary>
/// Регистрация и валидация Options для DI
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Регистрирует настройки NewsMaker с валидацией
    /// </summary>
    public static IServiceCollection AddNewsMakerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрация Options с привязкой к секции конфигурации
        services.AddOptions<NewsMakerOptions>()
            .BindConfiguration(NewsMakerOptions.SectionName)
            .ValidateOnStart();

        // Регистрация FluentValidation валидатора
        services.AddSingleton<IValidator<NewsMakerOptions>, NewsMakerOptionsValidator>();

        // Регистрация адаптера для IValidateOptions<T>
        services.AddSingleton<IValidateOptions<NewsMakerOptions>>(provider =>
        {
            var validator = provider.GetRequiredService<IValidator<NewsMakerOptions>>();
            return new FluentValidationOptions<NewsMakerOptions>(validator);
        });

        return services;
    }
}

/// <summary>
/// Адаптер FluentValidation для IValidateOptions<T>
/// </summary>
internal class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IValidator<TOptions> _validator;

    public FluentValidationOptions(IValidator<TOptions> validator)
    {
        _validator = validator;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        var validationResult = _validator.Validate(options);

        if (validationResult.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = validationResult.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

        return ValidateOptionsResult.Fail(errors);
    }
}
