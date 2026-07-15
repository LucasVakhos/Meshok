using System.ComponentModel.DataAnnotations;

namespace NewsWave.Mail;

public sealed class MailComposeInput
{
    [Required(ErrorMessage = "Добавьте хотя бы одного получателя.")]
    [Display(Name = "Получатели")]
    public string Recipients { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите тему письма.")]
    [StringLength(200, ErrorMessage = "Тема не должна быть длиннее 200 символов.")]
    [Display(Name = "Тема")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Напишите текст письма.")]
    [StringLength(200_000, ErrorMessage = "Текст письма слишком большой.")]
    [Display(Name = "Сообщение")]
    public string Body { get; set; } = string.Empty;

    [Display(Name = "HTML-письмо")]
    public bool IsHtml { get; set; }
}

public sealed record MailRequest(
    IReadOnlyList<string> Recipients,
    string Subject,
    string Body,
    bool IsHtml);
