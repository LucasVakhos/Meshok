using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsWave.Contacts;
using NewsWave.Data;

namespace NewsWave.Pages.Contacts;

public sealed class IndexModel : PageModel
{
    private readonly NewsWaveStore _store;

    public IndexModel(NewsWaveStore store)
    {
        _store = store;
    }

    [BindProperty]
    public ContactInput Input { get; set; } = new();

    public IReadOnlyList<ContactRecord> Contacts => _store.GetContacts();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _store.AddContactAsync(
                Input.Name,
                Input.Email,
                Input.Group,
                Input.IsActive,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.Email", ex.Message);
            return Page();
        }

        TempData["ContactSuccess"] = "Получатель добавлен в адресную книгу.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid deleteId, CancellationToken cancellationToken)
    {
        await _store.DeleteContactAsync(deleteId, cancellationToken);
        TempData["ContactSuccess"] = "Получатель удалён.";
        return RedirectToPage();
    }
}
