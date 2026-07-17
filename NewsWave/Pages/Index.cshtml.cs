using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.Data;
using NewsWave.NewsMaker;

namespace NewsWave.Pages;

public sealed class IndexModel : PageModel
{
    private readonly BridgeNoteRepository _database;
    private readonly NewsMakerSettingsStore _settings;
    private readonly INewsMakerRunner _runner;
    private readonly NewsWaveStore _store;

    public IndexModel(BridgeNoteRepository database, NewsMakerSettingsStore settings, INewsMakerRunner runner, NewsWaveStore store)
    {
        _database = database;
        _settings = settings;
        _runner = runner;
        _store = store;
    }

    public NewsMakerSnapshot Run { get; private set; } = new(NewsMakerRunStatus.Idle, "", 0, 0, null, null, []);
    public bool DatabaseConfigured { get; private set; }
    public bool DatabaseAvailable { get; private set; }
    public bool SmtpConfigured { get; private set; }
    public int SubscriberCount { get; private set; }
    public int QueueCount { get; private set; }
    public int TemplateCount { get; private set; }
    public SendInterval? Interval { get; private set; }
    public DateTime? NextRun { get; private set; }
    public string? DatabaseError { get; private set; }

    public async Task OnGetAsync(CancellationToken token)
    {
        NewsMakerSettings settings = _settings.Current;
        Run = _runner.Snapshot();
        DatabaseConfigured = settings.BridgeNote.IsConfigured;
        SmtpConfigured = settings.Post.IsConfigured;
        TemplateCount = _store.GetTemplates().Count;

        if (DatabaseConfigured)
        {
            try
            {
                await _database.TestAsync(token);
                DatabaseAvailable = true;
                SubscriberCount = (await _database.GetEmailsAsync(token)).Count;
                QueueCount = await _database.BufferCountAsync(token);
                Interval = await _database.ReadIntervalAsync(token);
            }
            catch (Exception ex) { DatabaseError = ex.Message; }
        }

        NextRun = CalculateNextRun(settings.Program, DateTime.Now);
    }

    private static DateTime? CalculateNextRun(ProgramSettings settings, DateTime now)
    {
        if (settings.RunDay == 7) return null;
        int days = (settings.RunDay - (int)now.DayOfWeek + 7) % 7;
        DateTime candidate = now.Date.AddDays(days).Add(settings.RunTime);
        return candidate <= now ? candidate.AddDays(7) : candidate;
    }
}
