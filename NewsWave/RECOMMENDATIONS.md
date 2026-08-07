# NewsWave - Краткие рекомендации

## 📊 Текущая оценка: **7/10**

### ✅ Что уже хорошо
- Современный .NET 8 + Razor Pages
- Async/await везде
- Dependency Injection
- Background Service для автоматики
- Health Checks

### ⚠️ Что нужно улучшить

## 🎯 ТОП-5 Приоритетных улучшений

### 1. **Repository Pattern + Dapper** (🔥 Критично)
**Проблема:** Прямая работа с ADO.NET, смешение data access и бизнес-логики

**Решение:**
```bash
dotnet add package Dapper
```

```csharp
public interface ISubscriberRepository
{
	Task<IReadOnlyList<string>> GetEmailsAsync(CancellationToken ct);
	Task AddAsync(string email, CancellationToken ct);
	Task DeleteAsync(string email, CancellationToken ct);
}

public class DapperSubscriberRepository : ISubscriberRepository
{
	private readonly string _connectionString;

	public async Task<IReadOnlyList<string>> GetEmailsAsync(CancellationToken ct)
	{
		await using var connection = new MySqlConnection(_connectionString);
		var sql = "SELECT DISTINCT TRIM(email) FROM subscribers WHERE email IS NOT NULL";
		return (await connection.QueryAsync<string>(sql)).ToList();
	}
}
```

**Выгода:** Чистая архитектура, легкое тестирование, меньше дублирования кода

---

### 2. **Options Pattern для конфигурации** (🔥 Критично)
**Проблема:** Прямое чтение из IConfiguration, нет валидации, нет type-safety

**Решение:**
```csharp
// 1. Создать класс опций
public class NewsMakerOptions
{
	public const string SectionName = "NewsMaker";

	[Required]
	public ProgramOptions Program { get; set; } = new();

	[Required]
	public BridgeNoteOptions BridgeNote { get; set; } = new();

	[Required]
	public SmtpOptions Post { get; set; } = new();

	[Range(1, 1000)]
	public int SendLimit { get; set; } = 10;
}

// 2. Зарегистрировать
builder.Services.AddOptions<NewsMakerOptions>()
	.BindConfiguration(NewsMakerOptions.SectionName)
	.ValidateDataAnnotations()
	.ValidateOnStart();

// 3. Использовать
public class MyService
{
	private readonly NewsMakerOptions _options;

	public MyService(IOptions<NewsMakerOptions> options)
	{
		_options = options.Value;
	}
}
```

**Выгода:** Type-safe конфигурация, ранняя валидация, IntelliSense

---

### 3. **User Secrets для паролей** (🔥 Security)
**Проблема:** Пароли в JSON файлах

**Решение:**
```bash
# Development
dotnet user-secrets init
dotnet user-secrets set "NewsMaker:BridgeNote:Password" "your-db-password"
dotnet user-secrets set "NewsMaker:Post:PassWrd" "your-smtp-password"

# Production (Environment Variables)
export NewsMaker__BridgeNote__Password="secure-password"
export NewsMaker__Post__PassWrd="smtp-password"
```

**Выгода:** Безопасность, никаких секретов в Git

---

### 4. **Polly для Retry Logic** (⚡ Важно)
**Проблема:** Нет retry для email отправки и БД операций

**Решение:**
```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

```csharp
// Registration
builder.Services.AddResiliencePipeline("email-retry", builder =>
{
	builder
		.AddRetry(new RetryStrategyOptions
		{
			MaxRetryAttempts = 3,
			Delay = TimeSpan.FromSeconds(2),
			BackoffType = DelayBackoffType.Exponential
		})
		.AddTimeout(TimeSpan.FromSeconds(30));
});

// Usage
public class EmailService
{
	private readonly ResiliencePipeline _pipeline;

	public async Task SendAsync(Email email)
	{
		await _pipeline.ExecuteAsync(async ct =>
		{
			// Send email
			await _smtpClient.SendAsync(email, ct);
		});
	}
}
```

**Выгода:** Устойчивость к временным сбоям, меньше failed emails

---

### 5. **Unit Tests** (⚡ Важно)
**Проблема:** Нет тестов вообще

**Решение:**
```bash
dotnet new xunit -n NewsWave.Tests
cd NewsWave.Tests
dotnet add reference ../NewsWave/NewsWave.csproj
dotnet add package FluentAssertions
dotnet add package Moq
```

```csharp
public class SubscriberRepositoryTests
{
	[Fact]
	public async Task GetEmailsAsync_ShouldReturnDistinctEmails()
	{
		// Arrange
		var repository = CreateRepository();

		// Act
		var emails = await repository.GetEmailsAsync(CancellationToken.None);

		// Assert
		emails.Should().OnlyHaveUniqueItems();
		emails.Should().AllSatisfy(e => e.Should().Contain("@"));
	}
}
```

**Выгода:** Уверенность в коде, меньше багов в production

---

## 🚀 Quick Wins (можно сделать за час)

### 1. Добавить Serilog (15 минут)
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

```csharp
builder.Host.UseSerilog((context, config) =>
{
	config
		.ReadFrom.Configuration(context.Configuration)
		.Enrich.FromLogContext()
		.WriteTo.Console()
		.WriteTo.File("logs/newswave-.log", rollingInterval: RollingInterval.Day);
});
```

### 2. Улучшить Health Checks (15 минут)
```csharp
builder.Services.AddHealthChecks()
	.AddCheck("database", () =>
	{
		// Test DB connection
		return HealthCheckResult.Healthy();
	})
	.AddCheck("smtp", () =>
	{
		// Test SMTP connection
		return HealthCheckResult.Healthy();
	});

app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = async (context, report) =>
	{
		context.Response.ContentType = "application/json";
		await context.Response.WriteAsJsonAsync(new
		{
			status = report.Status.ToString(),
			checks = report.Entries.Select(e => new
			{
				name = e.Key,
				status = e.Value.Status.ToString(),
				description = e.Value.Description
			})
		});
	}
});
```

### 3. Request Logging (5 минут)
```csharp
app.UseSerilogRequestLogging();
```

### 4. Rate Limiting (10 минут)
```csharp
builder.Services.AddRateLimiter(options =>
{
	options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
		PartitionedRateLimiter.Create<HttpContext, string>(context =>
			RateLimitPartition.GetFixedWindowLimiter(
				context.User.Identity?.Name ?? "anonymous",
				_ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = 100,
					Window = TimeSpan.FromMinutes(1)
				})));
});

app.UseRateLimiter();
```

---

## 📋 Дорожная карта

### Неделя 1: Foundation
- [ ] Repository Pattern + Dapper
- [ ] Options Pattern
- [ ] User Secrets
- [ ] Basic Unit Tests

### Неделя 2: Resilience
- [ ] Polly retry policies
- [ ] Email sender abstraction
- [ ] Improved error handling
- [ ] Integration tests

### Неделя 3: Observability
- [ ] Serilog structured logging
- [ ] Enhanced health checks
- [ ] Metrics endpoint
- [ ] Request logging

### Неделя 4: Polish
- [ ] Rate limiting
- [ ] Security headers
- [ ] Performance optimization
- [ ] Documentation update

---

## 📦 Рекомендуемые пакеты

```xml
<!-- Data Access -->
<PackageReference Include="Dapper" Version="2.1.66" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />

<!-- Resilience -->
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="8.0.0" />

<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.9.0" />

<!-- Testing -->
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="FluentAssertions" Version="8.10.0" />
<PackageReference Include="Moq" Version="4.20.72" />
```

---

## 🎓 Обучающие ресурсы

- [Repository Pattern](https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)
- [Options Pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Polly Documentation](https://www.pollydocs.org/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [xUnit Testing](https://xunit.net/)

---

## 💬 Резюме

**NewsWave** - хороший проект с современным стеком, но ему нужны:

1. **Repository Pattern** для чистой архитектуры
2. **Options Pattern** для type-safe конфигурации
3. **User Secrets** для безопасности
4. **Polly** для устойчивости
5. **Unit Tests** для качества

**Начните с пунктов 1-3** - они дадут максимальный эффект за минимальное время!

Полная документация в [MODERNIZATION_PLAN.md](MODERNIZATION_PLAN.md)
