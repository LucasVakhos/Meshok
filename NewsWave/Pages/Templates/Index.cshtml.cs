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

    public IReadOnlyList<MailTemplateRecord> Templates => _store.GetTemplates();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        await _store.AddTemplateAsync(
            Input.Name,
            Input.Subject,
            Input.Body,
            Input.IsHtml,
            cancellationToken);

        TempData["TemplateSuccess"] = "Шаблон сохранён.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid deleteId, CancellationToken cancellationToken)
    {
        await _store.DeleteTemplateAsync(deleteId, cancellationToken);
        TempData["TemplateSuccess"] = "Шаблон удалён.";
        return RedirectToPage();
    }
}
