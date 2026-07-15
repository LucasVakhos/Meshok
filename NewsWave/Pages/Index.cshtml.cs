using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NewsWave.Mail;

namespace NewsWave.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IMailQueue _mailQueue;
    private readonly IOptionsSnapshot<MailOptions> _mailOptions;

    public IndexModel(
        IMailQueue mailQueue,
        IOptionsSnapshot<MailOptions> mailOptions)
    {
        _mailQueue = mailQueue;
        _mailOptions = mailOptions;
    }

    public bool MailConfigured { get; private set; }
    public int ActiveCount { get; private set; }
    public int SentCount { get; private set; }
    public int FailedCount { get; private set; }

    public void OnGet()
    {
        IReadOnlyList<MailDispatchSnapshot> dispatches = _mailQueue.Recent(100);
        MailConfigured = _mailOptions.Value.IsConfigured;
        ActiveCount = dispatches.Count(x =>
            x.Status is MailDispatchStatus.Queued or MailDispatchStatus.Sending);
        SentCount = dispatches.Count(x => x.Status == MailDispatchStatus.Sent);
        FailedCount = dispatches.Count(x => x.Status == MailDispatchStatus.Failed);
    }
}
