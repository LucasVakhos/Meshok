using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.Data;
using NewsWave.Templates;

namespace NewsWave.Pages.Templates;

public sealed class IndexModel : PageModel
{
    private readonly NewsWaveStore _store;

    public IndexModel(NewsWaveStore store)
    {
        _store = store;
    }

    [BindProperty]
    public TemplateInput Input { get; set; } = new();

    [BindProperty]
    public Guid? EditId { get; set; }

    public bool IsEditing => EditId.HasValue;
    public IReadOnlyList<MailTemplateRecord> Templates => _store.GetTemplates();

    public IActionResult OnGet(Guid? editId)
    {
        if (editId is not Guid id)
            return Page();

        MailTemplateRecord? template = _store.FindTemplate(id);
        if (template is null)
            return RedirectToPage();

        EditId = template.Id;
        Input = new TemplateInput
        {
            Name = template.Name,
            Subject = template.Subject,
            Body = template.Body,
            IsHtml = template.IsHtml
        };
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        if (EditId is Guid id)
        {
            await _store.UpdateTemplateAsync(
                id, Input.Name, Input.Subject, Input.Body, Input.IsHtml, cancellationToken);
            TempData["TemplateSuccess"] = "Шаблон обновлён.";
        }
        else
        {
            await _store.AddTemplateAsync(
                Input.Name, Input.Subject, Input.Body, Input.IsHtml, cancellationToken);
            TempData["TemplateSuccess"] = "Шаблон сохранён.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid deleteId, CancellationToken cancellationToken)
    {
        await _store.DeleteTemplateAsync(deleteId, cancellationToken);
        TempData["TemplateSuccess"] = "Шаблон удалён.";
        return RedirectToPage();
    }
}
