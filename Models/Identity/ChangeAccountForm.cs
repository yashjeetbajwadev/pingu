using FluentValidation;
using pingu.Providers.ModelValidator;
using pingu.Providers.Validation;
using System.Text.Json.Serialization;

namespace pingu.Models.Identity;

public class SendChangeAccountCodeForm
{
    [JsonIgnore]
    public ContactType NewUsernameType => !string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : default;

    public string NewUsername { get; init; } = null!;
}

public class ChangeAccountForm
{
    [JsonIgnore]
    public ContactType NewUsernameType => !string.IsNullOrWhiteSpace(NewUsername) ? ValidationHelper.DetermineContactType(NewUsername) : default;

    public string NewUsername { get; init; } = null!;

    public string Code { get; init; } = null!;
}

public class SendChangeAccountCodeFormValidator : AbstractValidator<SendChangeAccountCodeForm>
{
    public SendChangeAccountCodeFormValidator()
    {
        RuleFor(f => f.NewUsername).NotEmpty().WithName("New email or phone number").DependentRules(() =>
        {
            When(f => f.NewUsernameType == ContactType.Email, () =>
            {
                RuleFor(f => f.NewUsername).Email().WithName("New email");
            });

            When(f => f.NewUsernameType == ContactType.PhoneNumber, () =>
            {
                RuleFor(f => f.NewUsername).PhoneNumber().WithName("New phone number");
            });
        });
    }
}

public class ChangeAccountFormValidator : AbstractValidator<ChangeAccountForm>
{
    public ChangeAccountFormValidator()
    {
        RuleFor(f => f.NewUsername).NotEmpty().WithName("New email or phone number").DependentRules(() =>
        {
            When(f => f.NewUsernameType == ContactType.Email, () =>
            {
                RuleFor(f => f.NewUsername).Email().WithName("New email");
            });

            When(f => f.NewUsernameType == ContactType.PhoneNumber, () =>
            {
                RuleFor(f => f.NewUsername).PhoneNumber().WithName("New phone number");
            });
        });

        RuleFor(f => f.Code).NotEmpty();
    }
}