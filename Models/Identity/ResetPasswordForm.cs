using FluentValidation;
using pingu.Providers.ModelValidator;
using pingu.Providers.Validation;
using System.Text.Json.Serialization;

namespace pingu.Models.Identity;

public class SendResetPasswordCodeForm
{
    public string Username { get; init; } = null!;

    [JsonIgnore]
    public ContactType UsernameType => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
}

public class ResetPasswordForm : SendResetPasswordCodeForm
{
    public string NewPassword { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;

    public string Code { get; init; } = null!;
}

public class SendResetPasswordCodeFormValidator : AbstractValidator<SendResetPasswordCodeForm>
{
    public SendResetPasswordCodeFormValidator()
    {
        RuleFor(f => f.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
        {
            When(f => f.UsernameType == ContactType.Email, () =>
            {
                RuleFor(f => f.Username).Email().WithName("Email");
            });

            When(f => f.UsernameType == ContactType.PhoneNumber, () =>
            {
                RuleFor(f => f.Username).PhoneNumber().WithName("Phone number");
            });
        });
    }
}

public class ResetPasswordFormValidator : AbstractValidator<ResetPasswordForm>
{
    public ResetPasswordFormValidator()
    {
        RuleFor(f => f.Username).NotEmpty().WithName("Email or phone number").DependentRules(() =>
        {
            When(f => f.UsernameType == ContactType.Email, () =>
            {
                RuleFor(f => f.Username).Email().WithName("Email");
            });

            When(f => f.UsernameType == ContactType.PhoneNumber, () =>
            {
                RuleFor(f => f.Username).PhoneNumber().WithName("Phone number");
            });
        });

        RuleFor(f => f.Code).NotEmpty();

        RuleFor(f => f.NewPassword).NotEmpty().Password();

        RuleFor(f => f.ConfirmPassword).NotEmpty().Equal(f => f.NewPassword);
    }
}