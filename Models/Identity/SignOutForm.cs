using FluentValidation;
using System.ComponentModel;

namespace pingu.Models.Identity;

public class SignOutForm
{
    public string RefreshToken { get; set; } = null!;

    [DefaultValue(true)]
    public bool AllowMultipleTokens { get; set; } = true;
}


public class SignOutFormValidator : AbstractValidator<SignOutForm>
{
    public SignOutFormValidator()
    {
        RuleFor(f => f.RefreshToken).NotEmpty();
    }
}