using AutoMapper;
using FluentValidation;
using pingu.Data.Entities.Identity;
using pingu.Providers.ModelValidator;
using pingu.Providers.Validation;
using System.Text.Json.Serialization;

namespace pingu.Models.Identity;

public class CreateAccountForm
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Username { get; init; } = null!;

    [JsonIgnore]
    public ContactType UsernameType => !string.IsNullOrWhiteSpace(Username) ? ValidationHelper.DetermineContactType(Username) : default;

    public string Password { get; set; } = null!;
}

public class CreateAccountFormValidator : AbstractValidator<CreateAccountForm>
{
    public CreateAccountFormValidator()
    {
        RuleFor(f => f.FirstName).NotEmpty().MaximumLength(256);
        RuleFor(f => f.LastName).MaximumLength(256);
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
        RuleFor(f => f.Password).NotEmpty().Password();
    }
}

public class CreateAccountFormProfile : Profile
{
    public CreateAccountFormProfile()
    {
        CreateMap<CreateAccountForm, User>()
            .ForMember(u => u.UserName, e => e.Ignore())
            .ForMember(u => u.Email, e => e.Ignore())
            .ForMember(u => u.PhoneNumber, e => e.Ignore());
    }
}