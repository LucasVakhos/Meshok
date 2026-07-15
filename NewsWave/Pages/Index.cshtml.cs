using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NewsWave.Data;
using NewsWave.Mail;

namespace NewsWave.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IMailQueue _mailQueue;
    private readonly IOptionsSnapshot<MailOptions> _mailOptions;
    private readonly NewsWaveStore _store;

    public IndexModel(
        IMailQueue mailQueue,
        IOptionsSnapshot<MailOptions> mailOptions,
        NewsWaveStore store)
    {
        _mailQueue = mailQueue;
        _mailOptions = mailOptions;
        _store = store;
    }

    public bool MailConfigured { get; private set; }
    public int ActiveCount { get; private set; }
    public int SentCount { get; private set; }
    public int FailedCount { get; private set; }
    public int ContactCount { get; private set; }
    public int TemplateCount { get; private set; }

    public void OnGet()
    {
        IReadOnlyList<MailDispatchSnapshot> dispatches = _mailQueue.Recent(100);
        MailConfigured = _mailOptions.Value.IsConfigured;
        ActiveCount = dispatches.Count(x => x.Status is MailDispatchStatus.Queued or MailDispatchStatus.Sending);
        SentCount = dispatches.Count(x => x.Status == MailDispatchStatus.Sent);
        FailedCount = dispatches.Count(x => x.Status == MailDispatchStatus.Failed);
        ContactCount = _store.GetContacts().Count(x => x.IsActive);
        TemplateCount = _store.GetTemplates().Count;
    }
}
