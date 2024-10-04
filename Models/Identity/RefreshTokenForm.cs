using FluentValidation;

namespace pingu.Models.Identity;

public class RefreshTokenForm
{
    public string RefreshToken { get; init; } = null!;
}

public class RefreshTokenFormValidator : AbstractValidator<RefreshTokenForm>
{
    public RefreshTokenFormValidator()
    {
        RuleFor(f => f.RefreshToken).NotEmpty();
    }
}