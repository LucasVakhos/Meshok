using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NewsWave.Pages;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration) => _configuration = configuration;

    [BindProperty, Required(ErrorMessage = "Введите пароль."), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");
        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        string? expected = _configuration["NewsWave:AdminPassword"];
        if (string.IsNullOrWhiteSpace(expected))
        {
            ModelState.AddModelError(string.Empty, "Пароль администратора не настроен. Локальный доступ выполняется без входа.");
            return Page();
        }

        if (!ModelState.IsValid || !PasswordsMatch(Password, expected))
        {
            ModelState.AddModelError(string.Empty, "Неверный пароль.");
            return Page();
        }

        ClaimsPrincipal principal = new(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Администратор NewsWave")],
            CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl)
            : RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage();
    }

    private static bool PasswordsMatch(string supplied, string expected)
    {
        byte[] suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(supplied));
        byte[] expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, expectedHash);
    }
}
