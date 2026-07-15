using System.ComponentModel.DataAnnotations;

namespace NewsWave.Contacts;

public sealed class ContactInput
{
    [Required(ErrorMessage = "Укажите имя получателя.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Некорректный email.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(80)]
    public string Group { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
