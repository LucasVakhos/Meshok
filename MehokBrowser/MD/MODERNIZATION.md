# Модернизация MehokBrowser

## Обзор

Данный документ описывает модернизацию проекта MehokBrowser к современным практикам .NET 8, сохраняя обратную совместимость с существующей кодовой базой.

## Выполненные улучшения

### 1. Современная инфраструктура

#### Host и Dependency Injection
- **Program.cs**: Переписан с использованием Generic Host (`Host.CreateDefaultBuilder()`)
- **DI контейнер**: Настроен контейнер Microsoft.Extensions.DependencyInjection
- **ServiceProvider**: Доступен глобально через `Program.ServiceProvider` для legacy кода
- **AppContextMB**: Обновлён для доступа к сервисам через `GetService<T>()` и `GetRequiredService<T>()`

#### Конфигурация
- **appsettings.json**: Создан основной файл конфигурации с:
  - Connection strings (Firebird, MySQL)
  - Logging configuration
  - Application metadata
  - Feature flags
  - Database settings
  - UI preferences
- **appsettings.Development.json**: Добавлен для development-специфичных настроек
- **ConfigurationService**: Создан type-safe сервис для доступа к конфигурации

#### Логирование
- **Microsoft.Extensions.Logging**: Интегрирован современный фреймворк логирования
- **Log4Net**: Подключён через Microsoft.Extensions.Logging.Log4Net.AspNetCore
- **LoggingService**: Создан wrapper для унифицированного логирования
- **GlobalExceptionHandler**: Обновлён для работы с ILogger

#### Новые сервисы
- **ConnectionStringProvider**: Type-safe доступ к connection strings
- **ConfigurationService**: Центральный сервис для доступа к конфигурации
- **LoggingService**: Обёртка над Microsoft.Extensions.Logging

### 2. Модернизация Data Layer

#### Асинхронные операции
Добавлены async методы в `DapperLookupRepository.cs`:
- `LoadActiveUsersAsync()`
- `LoadMessageSettingsAsync()`
- `SaveMessageSettingAsync()`
- `DeleteMessageSettingAsync()`
- `FindClientAsync()`
- `FindOrderAsync()`
- `HasOrderAsync()`
- `ImportClientAsync()`
- `ImportOrderAsync()`

Все async методы:
- Принимают `CancellationToken` для поддержки отмены
- Используют `await using` для правильного управления ресурсами
- Не удаляют существующие sync методы (обратная совместимость)

#### Repository модернизация
`DapperMessageSettingsRepository.cs`:
- Добавлены async методы: `GetAsync()`, `SelectAllAsync()`, `SaveAsync()`, `DeleteAsync()`
- Сохранены все legacy методы для совместимости с LB.Libs
- Добавлена XML документация

### 3. Исправления и улучшения

#### StatusRelation.cs
- Исправлены конфликты namespace через type aliases:
  ```csharp
  using DeliveryMethodEnum = Common.DeliveryMethod;
  using PaymentMethodEnum = Common.PaymentMethod;
  ```

#### GlobalExceptionHandler.cs
- Интегрирован с Microsoft.Extensions.Logging
- Сохранена совместимость с LB.Libs.Logger
- Улучшено логирование всех типов исключений

### 4. Тестирование

#### Новый тестовый проект
Создан `MehokBrowser.Tests` с:
- xUnit как тестовым фреймворком
- FluentAssertions для читаемых утверждений
- Moq для mocking зависимостей
- Тестами для новых сервисов:
  - `DapperMessageSettingsRepositoryTests`
  - `ConnectionStringProviderTests`
  - `ConfigurationServiceTests`

## Архитектурные принципы

### Обратная совместимость
Все изменения сделаны с сохранением совместимости:
- Legacy `LB.Libs.AppContext` продолжает работать
- Статические методы в `DapperLookupRepository` не удалены
- Существующий код продолжает работать без изменений
- Новая функциональность добавлена аддитивно

### Постепенная миграция
- Новые сервисы доступны через DI
- Legacy код может получить доступ к сервисам через `AppContextMB.GetService<T>()`
- Async методы добавлены рядом с sync версиями
- Переход на новые API может происходить постепенно

## Использование

### Получение сервисов в legacy коде

```csharp
// В любом месте приложения
var logger = AppContextMB.GetService<ILogger<MyClass>>();
var config = AppContextMB.GetRequiredService<IConfigurationService>();
```

### Использование async repository методов

```csharp
// Вместо sync версии
var messages = DapperLookupRepository.LoadMessageSettings();

// Можно использовать async версию
var messages = await DapperLookupRepository.LoadMessageSettingsAsync(cancellationToken);
```

### Доступ к конфигурации

```csharp
var configService = AppContextMB.GetRequiredService<IConfigurationService>();
var appName = configService.ApplicationName;
var timeout = configService.GetValue("Database:CommandTimeout", 30);
```

### Логирование

```csharp
var logger = AppContextMB.GetService<ILogger<MyClass>>();
logger?.LogInformation("Operation completed successfully");
logger?.LogError(exception, "Operation failed");
```

## Следующие шаги

### Рекомендуемые улучшения
1. Постепенный переход на async/await в UI слое (используя `async void` для event handlers)
2. Рефакторинг форм для получения зависимостей через DI
3. Добавление интеграционных тестов с тестовой БД
4. Миграция с `LB.Libs.Logger` на `Microsoft.Extensions.Logging` в legacy коде
5. Добавление health checks для мониторинга состояния приложения
6. Настройка structured logging (Serilog/NLog) для production

### Опциональные улучшения
- Добавление Polly для retry policies
- Настройка distributed tracing (OpenTelemetry)
- Миграция с Newtonsoft.Json на System.Text.Json
- Добавление FluentValidation для валидации данных
- Настройка code analysis (analyzers, StyleCop)

## Зависимости

### Новые пакеты
- Microsoft.Extensions.DependencyInjection (8.0.1)
- Microsoft.Extensions.Hosting (8.0.1)
- Microsoft.Extensions.Configuration.Json (8.0.1)
- Microsoft.Extensions.Logging.Log4Net.AspNetCore (8.0.0)

### Тестовые пакеты
- xunit (2.9.3)
- FluentAssertions (8.10.0)
- Moq (4.20.72)
- Microsoft.NET.Test.Sdk (17.14.1)

## Заметки по обслуживанию

### Важные файлы конфигурации
- `appsettings.json` - основная конфигурация
- `appsettings.Development.json` - development overrides
- `log4net.config` - конфигурация Log4Net (если используется)

### Connection Strings
Connection strings теперь хранятся в `appsettings.json` в секции `ConnectionStrings`:
- `Firebird` - для Firebird базы данных
- `MySQL` - для MySQL базы данных

Для production окружения рекомендуется хранить connection strings в:
- User Secrets (для development)
- Environment Variables (для production)
- Azure Key Vault / аналоги (для cloud)

## Совместимость

- **.NET Version**: .NET 8.0
- **Target Framework**: net8.0-windows
- **Windows Forms**: Поддерживается
- **Legacy LB.Libs**: Полная совместимость сохранена
- **DevExpress**: Совместимость сохранена

## Авторы и история

- **Дата модернизации**: Апрель 2026
- **Версия**: 2.0.0 (после модернизации)
- **Статус**: Production-ready с обратной совместимостью

---

## FAQ

**Q: Нужно ли переписывать существующий код?**  
A: Нет. Вся существующая функциональность продолжает работать. Новые возможности доступны по желанию.

**Q: Как получить доступ к DI сервисам из старого кода?**  
A: Используйте `AppContextMB.GetService<T>()` или `AppContextMB.GetRequiredService<T>()`.

**Q: Можно ли использовать sync и async методы вместе?**  
A: Да, sync методы сохранены для совместимости. Используйте async методы в новом коде.

**Q: Где хранить connection strings для production?**  
A: Используйте Environment Variables или Azure Key Vault вместо appsettings.json для production.

**Q: Нужно ли переходить на async/await сразу везде?**  
A: Нет. Переход может быть постепенным. Начните с новых функций и медленно мигрируйте критичные участки.
