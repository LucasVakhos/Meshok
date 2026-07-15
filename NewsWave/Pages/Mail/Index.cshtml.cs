using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.NewsMaker;

namespace NewsWave.Pages.Mail;

public sealed class IndexModel : PageModel
{
    private readonly NewsMakerSettingsStore _settingsStore;
    private readonly BridgeNoteRepository _database;
    private readonly INewsMakerRunner _runner;

    public IndexModel(NewsMakerSettingsStore settingsStore, BridgeNoteRepository database, INewsMakerRunner runner)
    {
        _settingsStore = settingsStore;
        _database = database;
        _runner = runner;
    }

    [BindProperty] public NewsMakerSettings Settings { get; set; } = new();
    public NewsMakerSnapshot Run { get; private set; } =
        new(NewsMakerRunStatus.Idle, "", 0, 0, null, null, []);
    public int SubscriberCount { get; private set; }
    public string? DatabaseError { get; private set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        Settings = _settingsStore.Current;
        Run = _runner.Snapshot();
        try { SubscriberCount = (await _database.GetEmailsAsync(token)).Count; }
        catch (Exception ex) { DatabaseError = ex.Message; }
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            Run = _runner.Snapshot();
            return Page();
        }
        await _settingsStore.SaveAsync(Settings, token);
        TempData["MailSuccess"] = "Настройки сохранены.";
        return RedirectToPage();
    }

    public IActionResult OnPostStart()
    {
        TempData["MailSuccess"] = _runner.Start() ? "Рассылка запущена." : "Рассылка уже выполняется.";
        return RedirectToPage();
    }

    public IActionResult OnPostStop()
    {
        _runner.Stop();
        TempData["MailSuccess"] = "Остановка запрошена.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendToMeAsync(CancellationToken token)
    {
        try
        {
            await _runner.SendReportAsync(token);
            TempData["MailSuccess"] = "Отчёт отправлен.";
        }
        catch (Exception ex) { TempData["MailError"] = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestDatabaseAsync(CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            Run = _runner.Snapshot();
            return Page();
        }
        try
        {
            await _settingsStore.SaveAsync(Settings, token);
            await _database.TestAsync(token);
            TempData["MailSuccess"] = "Настройки сохранены. Подключение к BridgeNote установлено.";
        }
        catch (Exception ex) { TempData["MailError"] = ex.Message; }
        return RedirectToPage();
    }
}
