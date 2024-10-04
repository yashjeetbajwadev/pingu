using pingu.Data.Entities.Identity;

namespace pingu.Providers.JwtBearer;

public class JwtToken
{
    public virtual User User { get; init; } = null!;

    public string UserId { get; init; } = null!;

    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string AccessTokenHash { get; init; } = null!;

    public DateTimeOffset AccessTokenExpiresAt { get; init; }

    public string RefreshTokenHash { get; init; } = null!;

    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
}