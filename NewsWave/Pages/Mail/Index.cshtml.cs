using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NewsWave.Data;
using NewsWave.Mail;
using System.Net.Mail;

namespace NewsWave.Pages.Mail;

public sealed class IndexModel : PageModel
{
    private const int MaxRecipients = 500;
    private readonly IMailQueue _mailQueue;
    private readonly IOptionsSnapshot<MailOptions> _options;
    private readonly NewsWaveStore _store;

    public IndexModel(
        IMailQueue mailQueue,
        IOptionsSnapshot<MailOptions> options,
        NewsWaveStore store)
    {
        _mailQueue = mailQueue;
        _options = options;
        _store = store;
    }

    [BindProperty]
    public MailComposeInput Input { get; set; } = new();

    public MailOptions Settings => _options.Value;
    public IReadOnlyList<MailDispatchSnapshot> Dispatches => _mailQueue.Recent();
    public int ActiveContactsCount => _store.GetContacts().Count(x => x.IsActive);

    public void OnGet(Guid? templateId, bool contacts = false)
    {
        if (templateId.HasValue && _store.FindTemplate(templateId.Value) is MailTemplateRecord template)
        {
            Input.Subject = template.Subject;
            Input.Body = template.Body;
            Input.IsHtml = template.IsHtml;
        }

        if (contacts)
        {
            Input.Recipients = string.Join(
                Environment.NewLine,
                _store.GetContacts().Where(x => x.IsActive).Select(x => x.Email));
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<string> recipients = ParseRecipients(Input.Recipients);
        if (recipients.Count == 0)
            ModelState.AddModelError("Input.Recipients", "Не найдено ни одного корректного адреса.");
        if (recipients.Count > MaxRecipients)
            ModelState.AddModelError("Input.Recipients", $"За один запуск можно отправить не больше {MaxRecipients} писем.");
        if (!Settings.IsConfigured)
            ModelState.AddModelError(string.Empty, "Сначала настройте SMTP для NewsWave.");

        if (!ModelState.IsValid)
            return Page();

        Guid dispatchId = await _mailQueue.EnqueueAsync(
            new MailRequest(recipients, Input.Subject.Trim(), Input.Body, Input.IsHtml),
            cancellationToken);

        TempData["MailSuccess"] =
            $"Рассылка на {recipients.Count} адресов поставлена в очередь. Код: {dispatchId.ToString()[..8]}.";
        return RedirectToPage();
    }

    private IReadOnlyList<string> ParseRecipients(string source)
    {
        string[] values = (source ?? string.Empty)
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> valid = new();
        HashSet<string> unique = new(StringComparer.OrdinalIgnoreCase);

        foreach (string value in values)
        {
            try
            {
                MailAddress address = new(value);
                if (address.Address != value)
                {
                    ModelState.AddModelError("Input.Recipients", $"Адрес «{value}» содержит имя. Оставьте только email.");
                    continue;
                }

                if (unique.Add(address.Address))
                    valid.Add(address.Address);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("Input.Recipients", $"Некорректный адрес: {value}");
            }
        }

        return valid;
    }
}
