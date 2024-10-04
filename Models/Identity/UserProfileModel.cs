using AutoMapper;
using pingu.Data.Entities.Identity;

namespace pingu.Models.Identity;

public class UserProfileModel
{
    public string Id { get; init; } = null!;

    public string FirstName { get; init; } = null!;

    public string? LastName { get; init; }

    public string UserName { get; init; } = null!;

    public string? Email { get; init; }

    public bool EmailConfirmed { get; init; }

    public string? PhoneNumber { get; init; }

    public bool PhoneNumberConfirmed { get; init; }

    public bool PasswordConfigured { get; init; }

    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}

public class UserProfileModelProfile : Profile
{
    public UserProfileModelProfile()
    {
        CreateMap<User, UserProfileModel>();
    }
}