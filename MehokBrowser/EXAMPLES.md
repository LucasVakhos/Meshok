# Примеры использования современной инфраструктуры

## Пример 1: Форма со списком сообщений

```csharp
using Microsoft.Extensions.Logging;
using MeshokBrowser.Data;
using MeshokBrowser.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeshokBrowser.Forms.Examples
{
	public partial class MessagesListForm : Form
	{
		private readonly ILogger<MessagesListForm> _logger;
		private readonly DapperMessageSettingsRepository _repository;
		private CancellationTokenSource _cts;

		// Legacy конструктор для существующего кода
		public MessagesListForm() : this(
			AppContextMB.GetService<ILogger<MessagesListForm>>(),
			AppContextMB.GetRequiredService<DapperMessageSettingsRepository>())
		{
		}

		// DI конструктор
		public MessagesListForm(
			ILogger<MessagesListForm> logger,
			DapperMessageSettingsRepository repository)
		{
			_logger = logger;
			_repository = repository;
			InitializeComponent();
		}

		private async void MessagesListForm_Load(object sender, EventArgs e)
		{
			_logger?.LogInformation("Messages list form loading");
			await LoadMessagesAsync();
		}

		private async void RefreshButton_Click(object sender, EventArgs e)
		{
			await LoadMessagesAsync();
		}

		private async Task LoadMessagesAsync()
		{
			// Cancel previous load if any
			_cts?.Cancel();
			_cts = new CancellationTokenSource();

			try
			{
				// UI feedback
				RefreshButton.Enabled = false;
				StatusLabel.Text = "Загрузка...";
				Cursor = Cursors.WaitCursor;

				_logger?.LogDebug("Loading messages from database");

				// Async load
				var messages = await _repository.SelectAllAsync(_cts.Token);

				// Update UI
				dataGridView.DataSource = messages;
				StatusLabel.Text = $"Загружено {messages.Count} сообщений";

				_logger?.LogInformation("Loaded {Count} messages", messages.Count);
			}
			catch (OperationCanceledException)
			{
				_logger?.LogInformation("Message loading cancelled");
				StatusLabel.Text = "Загрузка отменена";
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to load messages");
				StatusLabel.Text = "Ошибка загрузки";
				MessageBox.Show(
					$"Не удалось загрузить сообщения:\n{ex.Message}",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				RefreshButton.Enabled = true;
				Cursor = Cursors.Default;
			}
		}

		private async void SaveButton_Click(object sender, EventArgs e)
		{
			if (dataGridView.CurrentRow?.DataBoundItem is MessagesSet message)
			{
				await SaveMessageAsync(message);
			}
		}

		private async Task SaveMessageAsync(MessagesSet message)
		{
			try
			{
				SaveButton.Enabled = false;
				StatusLabel.Text = "Сохранение...";

				_logger?.LogDebug("Saving message {Id}", message.id);

				await _repository.SaveAsync(message);

				StatusLabel.Text = "Сохранено";
				_logger?.LogInformation("Message {Id} saved successfully", message.id);

				MessageBox.Show("Сообщение сохранено", "Успех",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to save message {Id}", message.id);
				StatusLabel.Text = "Ошибка сохранения";
				MessageBox.Show(
					$"Не удалось сохранить сообщение:\n{ex.Message}",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				SaveButton.Enabled = true;
			}
		}

		private async void DeleteButton_Click(object sender, EventArgs e)
		{
			if (dataGridView.CurrentRow?.DataBoundItem is MessagesSet message)
			{
				var result = MessageBox.Show(
					$"Удалить сообщение #{message.id}?",
					"Подтверждение",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					await DeleteMessageAsync(message);
					await LoadMessagesAsync(); // Refresh after delete
				}
			}
		}

		private async Task DeleteMessageAsync(MessagesSet message)
		{
			try
			{
				DeleteButton.Enabled = false;
				StatusLabel.Text = "Удаление...";

				_logger?.LogDebug("Deleting message {Id}", message.id);

				await _repository.DeleteAsync(message);

				StatusLabel.Text = "Удалено";
				_logger?.LogInformation("Message {Id} deleted successfully", message.id);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to delete message {Id}", message.id);
				StatusLabel.Text = "Ошибка удаления";
				MessageBox.Show(
					$"Не удалось удалить сообщение:\n{ex.Message}",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				DeleteButton.Enabled = true;
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_cts?.Cancel();
			base.OnFormClosing(e);
		}
	}
}
```

## Пример 2: Импорт данных с прогрессом

```csharp
using Microsoft.Extensions.Logging;
using MeshokBrowser.Data;
using MeshokBrowser.ScanObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MeshokBrowser.Workers.Examples
{
	public partial class ImportForm : Form
	{
		private readonly ILogger<ImportForm> _logger;
		private CancellationTokenSource _cts;

		public ImportForm()
		{
			_logger = AppContextMB.GetService<ILogger<ImportForm>>();
			InitializeComponent();
		}

		private async void ImportButton_Click(object sender, EventArgs e)
		{
			var orders = GetOrdersToImport(); // Your logic here
			await ImportOrdersAsync(orders);
		}

		private async Task ImportOrdersAsync(List<Order> orders)
		{
			_cts?.Cancel();
			_cts = new CancellationTokenSource();

			try
			{
				ImportButton.Enabled = false;
				CancelButton.Enabled = true;
				progressBar.Value = 0;
				progressBar.Maximum = orders.Count;

				_logger?.LogInformation("Starting import of {Count} orders", orders.Count);

				var progress = new Progress<ImportProgress>(p =>
				{
					progressBar.Value = p.Current;
					StatusLabel.Text = $"Импорт {p.Current} из {p.Total} ({p.Percentage}%)";

					if (p.CurrentOrder != null)
					{
						DetailsLabel.Text = $"Заказ #{p.CurrentOrder.co_id}";
					}
				});

				var imported = await ImportOrdersWithProgressAsync(
					orders, 
					progress, 
					_cts.Token);

				_logger?.LogInformation(
					"Import completed: {Success} success, {Failed} failed", 
					imported.Success, 
					imported.Failed);

				MessageBox.Show(
					$"Импорт завершён:\nУспешно: {imported.Success}\nОшибок: {imported.Failed}",
					"Импорт",
					MessageBoxButtons.OK,
					imported.Failed == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
			}
			catch (OperationCanceledException)
			{
				_logger?.LogInformation("Import cancelled by user");
				MessageBox.Show("Импорт отменён", "Отмена", 
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Import failed");
				MessageBox.Show(
					$"Ошибка импорта:\n{ex.Message}",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			finally
			{
				ImportButton.Enabled = true;
				CancelButton.Enabled = false;
				progressBar.Value = 0;
				StatusLabel.Text = "Готово";
			}
		}

		private async Task<ImportResult> ImportOrdersWithProgressAsync(
			List<Order> orders,
			IProgress<ImportProgress> progress,
			CancellationToken cancellationToken)
		{
			var result = new ImportResult();
			var total = orders.Count;

			for (int i = 0; i < total; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var order = orders[i];

				progress.Report(new ImportProgress
				{
					Current = i + 1,
					Total = total,
					Percentage = (i + 1) * 100 / total,
					CurrentOrder = order
				});

				try
				{
					// Import order
					await DapperLookupRepository.ImportOrderAsync(order, cancellationToken);
					result.Success++;

					_logger?.LogDebug("Order {OrderId} imported successfully", order.co_id);
				}
				catch (Exception ex)
				{
					result.Failed++;
					result.Errors.Add($"Заказ {order.co_id}: {ex.Message}");

					_logger?.LogWarning(ex, "Failed to import order {OrderId}", order.co_id);
				}
			}

			return result;
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			_cts?.Cancel();
			StatusLabel.Text = "Отмена...";
		}

		private List<Order> GetOrdersToImport()
		{
			// Your logic to get orders
			return new List<Order>();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_cts?.Cancel();
			base.OnFormClosing(e);
		}
	}

	public class ImportProgress
	{
		public int Current { get; set; }
		public int Total { get; set; }
		public int Percentage { get; set; }
		public Order CurrentOrder { get; set; }
	}

	public class ImportResult
	{
		public int Success { get; set; }
		public int Failed { get; set; }
		public List<string> Errors { get; } = new();
	}
}
```

## Пример 3: Background Worker с async

```csharp
using Microsoft.Extensions.Logging;
using MeshokBrowser.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeshokBrowser.Workers.Examples
{
	/// <summary>
	/// Background worker для периодической синхронизации
	/// </summary>
	public class SyncWorker : IDisposable
	{
		private readonly ILogger<SyncWorker> _logger;
		private readonly Timer _timer;
		private readonly TimeSpan _interval;
		private CancellationTokenSource _cts;
		private Task _currentTask;

		public SyncWorker(TimeSpan interval)
		{
			_logger = AppContextMB.GetService<ILogger<SyncWorker>>();
			_interval = interval;
			_timer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
		}

		public void Start()
		{
			_logger?.LogInformation("Starting sync worker with interval {Interval}", _interval);
			_cts = new CancellationTokenSource();
			_timer.Change(TimeSpan.Zero, _interval);
		}

		public void Stop()
		{
			_logger?.LogInformation("Stopping sync worker");
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			_cts?.Cancel();
			_currentTask?.Wait(TimeSpan.FromSeconds(10));
		}

		private void OnTimerElapsed(object state)
		{
			if (_currentTask?.IsCompleted == false)
			{
				_logger?.LogWarning("Previous sync task still running, skipping this cycle");
				return;
			}

			_currentTask = Task.Run(async () =>
			{
				try
				{
					await SyncDataAsync(_cts.Token);
				}
				catch (OperationCanceledException)
				{
					_logger?.LogInformation("Sync operation cancelled");
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Sync operation failed");
				}
			});
		}

		private async Task SyncDataAsync(CancellationToken cancellationToken)
		{
			_logger?.LogDebug("Starting data synchronization");

			// Sync messages
			var messages = await DapperLookupRepository.LoadMessageSettingsAsync(cancellationToken);
			_logger?.LogDebug("Synchronized {Count} messages", messages.Count);

			// Sync users
			var users = await DapperLookupRepository.LoadActiveUsersAsync(cancellationToken);
			_logger?.LogDebug("Synchronized {Count} users", users.Count);

			_logger?.LogInformation("Data synchronization completed successfully");
		}

		public void Dispose()
		{
			Stop();
			_timer?.Dispose();
			_cts?.Dispose();
		}
	}

	// Использование в приложении:
	public class MainForm : Form
	{
		private SyncWorker _syncWorker;

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Start background sync every 5 minutes
			_syncWorker = new SyncWorker(TimeSpan.FromMinutes(5));
			_syncWorker.Start();
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			_syncWorker?.Stop();
			_syncWorker?.Dispose();
			base.OnFormClosing(e);
		}
	}
}
```

## Пример 4: Batch операции с retry

```csharp
using Microsoft.Extensions.Logging;
using MeshokBrowser.Data;
using MeshokBrowser.ScanObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MeshokBrowser.Workers.Examples
{
	public class BatchImporter
	{
		private readonly ILogger<BatchImporter> _logger;
		private readonly int _maxRetries;
		private readonly TimeSpan _retryDelay;

		public BatchImporter(int maxRetries = 3, int retryDelaySeconds = 5)
		{
			_logger = AppContextMB.GetService<ILogger<BatchImporter>>();
			_maxRetries = maxRetries;
			_retryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
		}

		public async Task<BatchResult> ImportClientsAsync(
			int siteId,
			List<Client> clients,
			CancellationToken cancellationToken = default)
		{
			_logger?.LogInformation(
				"Starting batch import of {Count} clients for site {SiteId}", 
				clients.Count, 
				siteId);

			var result = new BatchResult { Total = clients.Count };
			var batches = clients.Chunk(100); // Process in batches of 100

			foreach (var batch in batches)
			{
				await Task.WhenAll(batch.Select(client => 
					ImportClientWithRetryAsync(siteId, client, result, cancellationToken)));
			}

			_logger?.LogInformation(
				"Batch import completed: {Success} success, {Failed} failed", 
				result.Success, 
				result.Failed);

			return result;
		}

		private async Task ImportClientWithRetryAsync(
			int siteId,
			Client client,
			BatchResult result,
			CancellationToken cancellationToken)
		{
			for (int attempt = 1; attempt <= _maxRetries; attempt++)
			{
				try
				{
					await DapperLookupRepository.ImportClientAsync(
						siteId, 
						client, 
						cancellationToken);

					result.Success++;

					_logger?.LogDebug(
						"Client {ClientId} imported successfully", 
						client.site_id);

					return;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex)
				{
					_logger?.LogWarning(
						ex, 
						"Failed to import client {ClientId}, attempt {Attempt}/{MaxRetries}", 
						client.site_id, 
						attempt, 
						_maxRetries);

					if (attempt >= _maxRetries)
					{
						result.Failed++;
						result.Errors.Add(new BatchError
						{
							ItemId = client.site_id?.ToString() ?? "unknown",
							Message = ex.Message,
							Exception = ex
						});
						return;
					}

					await Task.Delay(_retryDelay, cancellationToken);
				}
			}
		}
	}

	public class BatchResult
	{
		public int Total { get; set; }
		public int Success { get; set; }
		public int Failed { get; set; }
		public List<BatchError> Errors { get; } = new();

		public double SuccessRate => Total > 0 ? (double)Success / Total * 100 : 0;
	}

	public class BatchError
	{
		public string ItemId { get; set; }
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
}
```

## Пример 5: Кэширование с invalidation

```csharp
using Microsoft.Extensions.Logging;
using MeshokBrowser.Data;
using MeshokBrowser.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MeshokBrowser.Services.Examples
{
	public class CachedMessageService
	{
		private readonly ILogger<CachedMessageService> _logger;
		private readonly DapperMessageSettingsRepository _repository;
		private List<MessagesSet> _cache;
		private DateTime _cacheExpiry;
		private readonly TimeSpan _cacheDuration;
		private readonly SemaphoreSlim _lock;

		public CachedMessageService(
			DapperMessageSettingsRepository repository,
			TimeSpan? cacheDuration = null)
		{
			_logger = AppContextMB.GetService<ILogger<CachedMessageService>>();
			_repository = repository;
			_cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(5);
			_lock = new SemaphoreSlim(1, 1);
		}

		public async Task<List<MessagesSet>> GetMessagesAsync(
			CancellationToken cancellationToken = default)
		{
			await _lock.WaitAsync(cancellationToken);

			try
			{
				if (_cache == null || DateTime.UtcNow >= _cacheExpiry)
				{
					_logger?.LogDebug("Cache expired, reloading messages");

					_cache = await _repository.SelectAllAsync(cancellationToken);
					_cacheExpiry = DateTime.UtcNow.Add(_cacheDuration);

					_logger?.LogInformation(
						"Messages cached, expires at {Expiry}", 
						_cacheExpiry);
				}
				else
				{
					_logger?.LogDebug("Returning cached messages");
				}

				return _cache;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task SaveMessageAsync(
			MessagesSet message,
			CancellationToken cancellationToken = default)
		{
			await _repository.SaveAsync(message, cancellationToken);

			// Invalidate cache
			await _lock.WaitAsync(cancellationToken);
			try
			{
				_cache = null;
				_logger?.LogDebug("Cache invalidated after save");
			}
			finally
			{
				_lock.Release();
			}
		}

		public void InvalidateCache()
		{
			_lock.Wait();
			try
			{
				_cache = null;
				_logger?.LogDebug("Cache invalidated manually");
			}
			finally
			{
				_lock.Release();
			}
		}
	}
}
```

Эти примеры показывают:
1. ✅ Правильное использование async/await
2. ✅ Обработку cancellation
3. ✅ Progress reporting
4. ✅ Retry логику
5. ✅ Background tasks
6. ✅ Кэширование
7. ✅ Логирование
8. ✅ Error handling
9. ✅ Resource cleanup
