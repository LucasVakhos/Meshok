using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.Data;
using NewsWave.NewsMaker;

namespace NewsWave.Pages.Mail;

public sealed class IndexModel : PageModel
{
    private readonly NewsMakerSettingsStore _settingsStore;
    private readonly BridgeNoteRepository _database;
    private readonly INewsMakerRunner _runner;
    private readonly NewsWaveStore _store;
    private readonly NewsletterArchiveBuilder _archiveBuilder;
    private readonly NewsletterSmtpSender _sender;

    public IndexModel(NewsMakerSettingsStore settingsStore, BridgeNoteRepository database, INewsMakerRunner runner,
        NewsWaveStore store, NewsletterArchiveBuilder archiveBuilder, NewsletterSmtpSender sender)
    {
        _settingsStore = settingsStore;
        _database = database;
        _runner = runner;
        _store = store;
        _archiveBuilder = archiveBuilder;
        _sender = sender;
    }

    [BindProperty] public NewsMakerSettings Settings { get; set; } = new();
    [BindProperty] public CampaignSettings Campaign { get; set; } = new();
    public IReadOnlyList<MailTemplateRecord> Templates => _store.GetTemplates();
    public NewsMakerSnapshot Run { get; private set; } =
        new(NewsMakerRunStatus.Idle, "", 0, 0, null, null, []);
    public int SubscriberCount { get; private set; }
    public string? DatabaseError { get; private set; }

    public async Task OnGetAsync(Guid? templateId, CancellationToken token)
    {
        Settings = _settingsStore.Current;
        Campaign = Settings.Campaign;
        Settings.BridgeNote.Password = null;
        Settings.Post.PassWrd = null;
        if (templateId is Guid id && _store.FindTemplate(id) is MailTemplateRecord template)
            Campaign = new CampaignSettings
            {
                TemplateId = template.Id,
                Subject = template.Subject,
                Body = template.Body,
                IsHtml = template.IsHtml
            };
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
        NewsMakerSettings current = _settingsStore.Current;
        if (string.IsNullOrWhiteSpace(Settings.BridgeNote.Password))
            Settings.BridgeNote.Password = current.BridgeNote.Password;
        if (string.IsNullOrWhiteSpace(Settings.Post.PassWrd))
            Settings.Post.PassWrd = current.Post.PassWrd;
        Settings.Campaign = current.Campaign;
        await _settingsStore.SaveAsync(Settings, token);
        TempData["MailSuccess"] = "Настройки сохранены.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveCampaignAsync(CancellationToken token)
    {
        if (!ValidateCampaign())
        {
            Settings = _settingsStore.Current;
            Run = _runner.Snapshot();
            return Page();
        }

        NewsMakerSettings settings = _settingsStore.Current;
        settings.Campaign = Campaign;
        await _settingsStore.SaveAsync(settings, token);
        TempData["MailSuccess"] = "Письмо сохранено и будет использовано в следующей рассылке.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendTestAsync(CancellationToken token)
    {
        if (!ValidateCampaign())
        {
            Settings = _settingsStore.Current;
            Run = _runner.Snapshot();
            return Page();
        }

        try
        {
            NewsMakerSettings settings = _settingsStore.Current;
            settings.Campaign = Campaign;
            await _settingsStore.SaveAsync(settings, token);
            (string text, string html) = _archiveBuilder.Personalize(
                "Иван", "https://bridgenote.com/unsubscribe/test", Campaign);
            string subject = NewsletterArchiveBuilder.PersonalizeSubject("Иван", Campaign);
            await _sender.SendTestAsync(subject, text, html, token);
            TempData["MailSuccess"] = "Тестовое письмо отправлено.";
        }
        catch (Exception ex) { TempData["MailError"] = ex.Message; }
        return RedirectToPage();
    }

    private bool ValidateCampaign()
    {
        if (string.IsNullOrWhiteSpace(Campaign.Subject))
            ModelState.AddModelError("Campaign.Subject", "Укажите тему письма.");
        if (string.IsNullOrWhiteSpace(Campaign.Body))
            ModelState.AddModelError("Campaign.Body", "Добавьте текст письма.");
        return ModelState.IsValid;
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
            NewsMakerSettings current = _settingsStore.Current;
            if (string.IsNullOrWhiteSpace(Settings.BridgeNote.Password))
                Settings.BridgeNote.Password = current.BridgeNote.Password;
            if (string.IsNullOrWhiteSpace(Settings.Post.PassWrd))
                Settings.Post.PassWrd = current.Post.PassWrd;
            Settings.Campaign = current.Campaign;
            await _settingsStore.SaveAsync(Settings, token);
            await _database.TestAsync(token);
            TempData["MailSuccess"] = "Настройки сохранены. Подключение к BridgeNote установлено.";
        }
        catch (Exception ex) { TempData["MailError"] = ex.Message; }
        return RedirectToPage();
    }
}
