using FluentValidation;

namespace NewsWave.Configuration;

/// <summary>
/// Валидатор настроек NewsMaker
/// </summary>
public class NewsMakerOptionsValidator : AbstractValidator<NewsMakerOptions>
{
    public NewsMakerOptionsValidator()
    {
        RuleFor(x => x.SendLimit)
            .GreaterThan(0)
            .WithMessage("SendLimit должен быть больше 0");

        RuleFor(x => x.SendLimit)
            .LessThanOrEqualTo(10000)
            .WithMessage("SendLimit не должен превышать 10000");

        RuleFor(x => x.ExportPath)
            .NotEmpty()
            .WithMessage("ExportPath не может быть пустым");

        RuleFor(x => x.Program.RunDay)
            .InclusiveBetween(1, 7)
            .WithMessage("RunDay должен быть от 1 до 7");

        RuleFor(x => x.Program.RunTime)
            .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromDays(1))
            .WithMessage("RunTime должно быть в пределах суток");

        // BridgeNote validation
        When(x => x.BridgeNote.IsConfigured, () =>
        {
            RuleFor(x => x.BridgeNote.Server)
                .NotEmpty()
                .WithMessage("BridgeNote.Server обязателен");

            RuleFor(x => x.BridgeNote.Database)
                .NotEmpty()
                .WithMessage("BridgeNote.Database обязателен");

            RuleFor(x => x.BridgeNote.UserID)
                .NotEmpty()
                .WithMessage("BridgeNote.UserID обязателен");

            RuleFor(x => x.BridgeNote.Port)
                .InclusiveBetween(1, 65535)
                .WithMessage("BridgeNote.Port должен быть от 1 до 65535");
        });

        // SMTP validation
        When(x => x.Post.IsConfigured, () =>
        {
            RuleFor(x => x.Post.Smtp)
                .NotEmpty()
                .WithMessage("Post.Smtp обязателен");

            RuleFor(x => x.Post.User)
                .NotEmpty()
                .WithMessage("Post.User обязателен");

            RuleFor(x => x.Post.BridgeEmail)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Post.BridgeEmail должен быть валидным email");

            RuleFor(x => x.Post.Port)
                .InclusiveBetween(1, 65535)
                .WithMessage("Post.Port должен быть от 1 до 65535");

            When(x => !string.IsNullOrWhiteSpace(x.Post.DeveloperEmail), () =>
            {
                RuleFor(x => x.Post.DeveloperEmail)
                    .EmailAddress()
                    .WithMessage("Post.DeveloperEmail должен быть валидным email");
            });
        });
    }
}
