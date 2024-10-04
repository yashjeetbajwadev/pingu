using AutoMapper;
using FluentValidation;
using pingu.Data.Entities.Identity;
using pingu.Providers.ModelValidator;
using pingu.Providers.Validation;
using System.Text.Json.Serialization;

namespace pingu.Models.Identity;

public class SignInWithProviderForm
{
    public SignInWithProvider Provider { get; init; }

    public string ProviderKey { get; init; } = null!;

    public string FirstName { get; init; } = null!;

    public string? LastName { get; init; }

    public string Username { get; init; } = null!;

    [JsonIgnore]
    public ContactType UsernameType => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;
}

public class SignInWithoutPasswordFormValidator : AbstractValidator<SignInWithProviderForm>
{
    public SignInWithoutPasswordFormValidator()
    {
        RuleFor(f => f.FirstName).NotEmpty().MaximumLength(256);

        RuleFor(f => f.Username).NotEmpty().DependentRules(() =>
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

public class SignInWithoutPasswordProfile : Profile
{
    public SignInWithoutPasswordProfile()
    {
        CreateMap<SignInForm, User>()
            .ForMember(u => u.UserName, e => e.Ignore())
            .ForMember(u => u.Email, e => e.Ignore())
            .ForMember(u => u.PhoneNumber, e => e.Ignore());
    }
}

public enum SignInWithProvider
{
    Google
}