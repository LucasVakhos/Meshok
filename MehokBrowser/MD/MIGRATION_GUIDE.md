# Migration Guide: Использование новой инфраструктуры

## Быстрый старт

### 1. Получение сервисов в legacy коде

```csharp
// Получить логгер
var logger = AppContextMB.GetService<ILogger<MyClass>>();
if (logger != null)
{
	logger.LogInformation("Something happened");
}

// Получить обязательный сервис (выбросит исключение если не найден)
var configService = AppContextMB.GetRequiredService<IConfigurationService>();
```

### 2. Логирование

#### Legacy способ (LB.Libs.Logger)
```csharp
Logger.Error("Error message", exception);
```

#### Новый способ (ILogger)
```csharp
var logger = AppContextMB.GetService<ILogger<MyClass>>();
logger?.LogError(exception, "Error message");
```

#### Через LoggingService
```csharp
var loggingService = AppContextMB.GetService<ILoggingService>();
loggingService?.Error("Error message", exception);
```

### 3. Конфигурация

#### Legacy способ (IniHelper)
```csharp
var cfg = IniHelper.Cfg<CfgIShop>();
var connectionString = cfg.ConnectionString();
```

#### Новый способ
```csharp
// Через ConnectionStringProvider
var provider = AppContextMB.GetService<IConnectionStringProvider>();
var connectionString = provider.GetFirebirdConnectionString();

// Через ConfigurationService
var config = AppContextMB.GetService<IConfigurationService>();
var appName = config.ApplicationName;
var timeout = config.GetValue("Database:CommandTimeout", 30);
```

### 4. Async операции с БД

#### Legacy sync способ
```csharp
var messages = DapperLookupRepository.LoadMessageSettings();
```

#### Новый async способ
```csharp
var messages = await DapperLookupRepository.LoadMessageSettingsAsync();
```

#### В event handler (WinForms)
```csharp
private async void LoadButton_Click(object sender, EventArgs e)
{
	try
	{
		LoadButton.Enabled = false;
		Cursor = Cursors.WaitCursor;

		var messages = await DapperLookupRepository.LoadMessageSettingsAsync();

		// Update UI
		dataGridView.DataSource = messages;
	}
	catch (Exception ex)
	{
		var logger = AppContextMB.GetService<ILogger<MyForm>>();
		logger?.LogError(ex, "Failed to load messages");
		MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", 
			MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
	finally
	{
		LoadButton.Enabled = true;
		Cursor = Cursors.Default;
	}
}
```

## Паттерны миграции

### Паттерн 1: Форма с DI зависимостями

```csharp
public partial class ModernForm : Form
{
	private readonly ILogger<ModernForm> _logger;
	private readonly IConfigurationService _config;

	// Конструктор для legacy создания (без DI)
	public ModernForm() : this(
		AppContextMB.GetService<ILogger<ModernForm>>(),
		AppContextMB.GetService<IConfigurationService>())
	{
	}

	// Конструктор для DI
	public ModernForm(ILogger<ModernForm> logger, IConfigurationService config)
	{
		_logger = logger;
		_config = config;
		InitializeComponent();
	}

	private void LoadData()
	{
		_logger?.LogInformation("Loading data...");
		var timeout = _config?.GetValue("Database:CommandTimeout", 30) ?? 30;
		// ... load data with timeout
	}
}
```

### Паттерн 2: Async load с CancellationToken

```csharp
public partial class DataForm : Form
{
	private CancellationTokenSource _cts;

	private async void LoadButton_Click(object sender, EventArgs e)
	{
		await LoadDataAsync();
	}

	private async Task LoadDataAsync()
	{
		// Cancel previous operation if any
		_cts?.Cancel();
		_cts = new CancellationTokenSource();

		try
		{
			var data = await DapperLookupRepository.LoadMessageSettingsAsync(_cts.Token);
			dataGridView.DataSource = data;
		}
		catch (OperationCanceledException)
		{
			// User cancelled
		}
		catch (Exception ex)
		{
			var logger = AppContextMB.GetService<ILogger<DataForm>>();
			logger?.LogError(ex, "Failed to load data");
			MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
				MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void CancelButton_Click(object sender, EventArgs e)
	{
		_cts?.Cancel();
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		_cts?.Cancel();
		base.OnFormClosing(e);
	}
}
```

### Паттерн 3: Progress reporting

```csharp
public partial class ImportForm : Form
{
	private async void ImportButton_Click(object sender, EventArgs e)
	{
		var progress = new Progress<int>(value =>
		{
			progressBar.Value = value;
			statusLabel.Text = $"Обработано {value}%";
		});

		await ImportDataAsync(progress);
	}

	private async Task ImportDataAsync(IProgress<int> progress)
	{
		var items = GetItemsToImport();
		var total = items.Count;

		for (int i = 0; i < total; i++)
		{
			await ProcessItemAsync(items[i]);
			progress.Report((i + 1) * 100 / total);
		}
	}
}
```

## Постепенная миграция

### Этап 1: Добавление логирования
Начните с добавления логирования в критичные места:

```csharp
// В начало метода
var logger = AppContextMB.GetService<ILogger<MyClass>>();

// При обработке ошибок
catch (Exception ex)
{
	logger?.LogError(ex, "Operation failed: {Operation}", operationName);
	throw;
}
```

### Этап 2: Миграция на async в новых методах
Все новые методы пишите с async/await:

```csharp
private async Task<List<Order>> LoadOrdersAsync()
{
	var logger = AppContextMB.GetService<ILogger<OrderManager>>();
	logger?.LogInformation("Loading orders...");

	try
	{
		return await DapperLookupRepository.LoadOrdersAsync();
	}
	catch (Exception ex)
	{
		logger?.LogError(ex, "Failed to load orders");
		throw;
	}
}
```

### Этап 3: Рефакторинг существующих методов
Постепенно переписывайте sync методы в async:

#### Before
```csharp
private void LoadData()
{
	var data = DapperLookupRepository.LoadMessageSettings();
	dataGridView.DataSource = data;
}
```

#### After
```csharp
private async Task LoadDataAsync()
{
	var data = await DapperLookupRepository.LoadMessageSettingsAsync();
	dataGridView.DataSource = data;
}

private async void LoadButton_Click(object sender, EventArgs e)
{
	await LoadDataAsync();
}
```

## Чего избегать

### ❌ Не делайте так:

```csharp
// 1. Blocking на async методах
var result = LoadDataAsync().Result; // DEADLOCK!
var result = LoadDataAsync().GetAwaiter().GetResult(); // DEADLOCK!

// 2. async void кроме event handlers
public async void LoadData() // BAD! Exceptions не могут быть caught
{
	await LoadDataAsync();
}

// 3. Смешивание sync и async без Task.Run
public void LoadData()
{
	var data = LoadDataAsync().Result; // DEADLOCK!
}
```

### ✅ Делайте так:

```csharp
// 1. Async all the way
public async Task LoadDataAsync()
{
	var data = await DapperLookupRepository.LoadMessageSettingsAsync();
	// ...
}

// 2. async void только для event handlers
private async void Button_Click(object sender, EventArgs e)
{
	try
	{
		await LoadDataAsync();
	}
	catch (Exception ex)
	{
		// Handle exception
	}
}

// 3. Если нужен sync wrapper для legacy кода
public void LoadData()
{
	// Используйте Task.Run для запуска в background потоке
	Task.Run(async () => await LoadDataAsync()).Wait();
}
```

## Тестирование

### Unit тесты с async

```csharp
[Fact]
public async Task LoadMessagesAsync_ShouldReturnMessages()
{
	// Arrange
	var repository = new DapperMessageSettingsRepository();

	// Act
	var messages = await repository.SelectAllAsync();

	// Assert
	messages.Should().NotBeNull();
	messages.Should().NotBeEmpty();
}
```

### Интеграционные тесты с БД

```csharp
[Fact]
public async Task SaveMessageAsync_ShouldPersistToDatabase()
{
	// Arrange
	var message = new MessagesSet
	{
		zsc_cs_id = 1,
		zsc_zs_id = 2,
		zsc_message = "Test"
	};

	// Act
	await DapperLookupRepository.SaveMessageSettingAsync(message);
	var saved = await DapperLookupRepository.LoadMessageSettingsAsync();

	// Assert
	saved.Should().Contain(m => m.zsc_message == "Test");

	// Cleanup
	await DapperLookupRepository.DeleteMessageSettingAsync(message.id);
}
```

## Полезные ссылки

- [async/await Best Practices](https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Configuration in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [Logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
