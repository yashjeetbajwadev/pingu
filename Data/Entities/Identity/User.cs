using Microsoft.AspNetCore.Identity;

namespace pingu.Data.Entities.Identity;

public sealed class User : IdentityUser<string>
{
    public User()
    {
        Id = Guid.NewGuid().ToString("N");
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastActiveAt { get; set; }

    public bool PasswordConfigured { get; set; }
}

public sealed class UserRole : IdentityUserRole<string>
{
    public User User { get; init; } = null!;

    public Role Role { get; init; } = null!;
}