using Microsoft.AspNetCore.Identity;

namespace pingu.Data.Entities.Identity;

public class Role : IdentityRole<string>
{
    public Role()
    {
        Id = Guid.NewGuid().ToString("N");
    }

    public Role(string roleName) : this()
    {
        Name = roleName;
    }


    public virtual ICollection<UserRole> UserRoles { get; init; } = new List<UserRole>();
}

public static class RoleNames
{
    public static string Administrator { get; set; } = nameof(Administrator);

    public static string Member { get; set; } = nameof(Member);

    public static string[] All => [Administrator, Member];
}