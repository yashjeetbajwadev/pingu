using FluentValidation;
using pingu.Providers.ModelValidator;

namespace pingu.Models.Identity;

public class ChangePasswordForm
{
    public string? OldPassword { get; set; }

    public string NewPassword { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;
}

public class ChangePasswordFormValidator : AbstractValidator<ChangePasswordForm>
{
    public ChangePasswordFormValidator()
    {
        RuleFor(f => f.NewPassword).NotEmpty().MaximumLength(128).Password();

        RuleFor(f => f.ConfirmPassword).NotEmpty().MaximumLength(128).Equal(f => f.NewPassword)
            .WithMessage("'Confirm password' must be equal to 'New password'");
    }
}