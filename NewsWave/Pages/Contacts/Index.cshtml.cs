using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using NewsWave.NewsMaker;
using System.ComponentModel.DataAnnotations;

namespace NewsWave.Pages.Contacts;

public sealed class IndexModel : PageModel
{
    private readonly BridgeNoteRepository _database;
    public IndexModel(BridgeNoteRepository database) => _database = database;

    [BindProperty, Required(ErrorMessage = "Укажите email."), EmailAddress(ErrorMessage = "Некорректный email.")]
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<string> Emails { get; private set; } = [];
    public string? DatabaseError { get; private set; }

    public async Task OnGetAsync(CancellationToken token) => await LoadAsync(token);

    public async Task<IActionResult> OnPostAddAsync(CancellationToken token)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync(token);
            return Page();
        }
        try
        {
            await _database.AddEmailAsync(Email, token);
            TempData["ContactSuccess"] = "Email добавлен в subscribers.";
            return RedirectToPage();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            ModelState.AddModelError(nameof(Email), "Такой email уже есть в subscribers.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        await LoadAsync(token);
        return Page();
    }

    private async Task LoadAsync(CancellationToken token)
    {
        try { Emails = await _database.GetEmailsAsync(token); }
        catch (Exception ex) { DatabaseError = ex.Message; }
    }
}
