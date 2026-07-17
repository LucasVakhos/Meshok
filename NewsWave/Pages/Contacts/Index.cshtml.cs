using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using NewsWave.NewsMaker;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace NewsWave.Pages.Contacts;

public sealed class IndexModel : PageModel
{
    private readonly BridgeNoteRepository _database;
    public IndexModel(BridgeNoteRepository database) => _database = database;

    [BindProperty, Required(ErrorMessage = "Укажите email."), EmailAddress(ErrorMessage = "Некорректный email.")]
    public string Email { get; set; } = string.Empty;
    [BindProperty] public IFormFile? ImportFile { get; set; }
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

    public async Task<IActionResult> OnPostDeleteAsync(string deleteEmail, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(deleteEmail))
        {
            TempData["ContactError"] = "Не выбран email для удаления.";
            return RedirectToPage();
        }

        try
        {
            await _database.DeleteEmailAsync(deleteEmail, token);
            TempData["ContactSuccess"] = $"Email {deleteEmail} удалён.";
        }
        catch (Exception ex) { TempData["ContactError"] = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostImportAsync(CancellationToken token)
    {
        if (ImportFile is null || ImportFile.Length == 0)
        {
            TempData["ContactError"] = "Выберите CSV или TXT файл.";
            return RedirectToPage();
        }
        if (ImportFile.Length > 2 * 1024 * 1024)
        {
            TempData["ContactError"] = "Файл импорта не должен превышать 2 МБ.";
            return RedirectToPage();
        }

        string extension = Path.GetExtension(ImportFile.FileName).ToLowerInvariant();
        if (extension is not (".csv" or ".txt"))
        {
            TempData["ContactError"] = "Поддерживаются только файлы CSV и TXT.";
            return RedirectToPage();
        }

        using StreamReader reader = new(ImportFile.OpenReadStream());
        string content = await reader.ReadToEndAsync(token);
        string[] candidates = content.Split([',', ';', '\t', '\r', '\n', ' '],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string[] emails = candidates
            .Select(x => x.Trim('"', (char)39, ' '))
            .Where(x => MailAddress.TryCreate(x, out MailAddress? parsed) &&
                        string.Equals(parsed.Address, x, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        int added = 0;
        int skipped = 0;
        try
        {
            foreach (string email in emails)
            {
                try
                {
                    await _database.AddEmailAsync(email, token);
                    added++;
                }
                catch (InvalidOperationException) { skipped++; }
            }
            TempData["ContactSuccess"] = $"Импорт завершён: добавлено {added}, пропущено {skipped}.";
        }
        catch (Exception ex) { TempData["ContactError"] = ex.Message; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken token)
    {
        try { Emails = await _database.GetEmailsAsync(token); }
        catch (Exception ex) { DatabaseError = ex.Message; }
    }
}
