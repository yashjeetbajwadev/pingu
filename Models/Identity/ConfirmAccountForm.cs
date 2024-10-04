using FluentValidation;
using pingu.Providers.ModelValidator;
using pingu.Providers.Validation;
using System.Text.Json.Serialization;

namespace pingu.Models.Identity;

public class SendConfirmAccountCodeForm
{
    public string Username { get; init; } = null!;

    [JsonIgnore]
    public ContactType UsernameType => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
}

public class ConfirmAccountForm
{
    public string Username { get; init; } = null!;

    [JsonIgnore]
    public ContactType UsernameType => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;

    public string Code { get; init; } = null!;
}

public class SendConfirmAccountCodeFormValidator : AbstractValidator<SendConfirmAccountCodeForm>
{
    public SendConfirmAccountCodeFormValidator()
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

public class ConfirmAccountFormValidator : AbstractValidator<ConfirmAccountForm>
{
    public ConfirmAccountFormValidator()
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
    }
}