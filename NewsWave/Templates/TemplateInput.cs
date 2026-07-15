using System.ComponentModel.DataAnnotations;

namespace NewsWave.Templates;

public sealed class TemplateInput
{
    [Required(ErrorMessage = "Укажите название шаблона.")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите тему письма.")]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Добавьте текст письма.")]
    [StringLength(200_000)]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; }
}
