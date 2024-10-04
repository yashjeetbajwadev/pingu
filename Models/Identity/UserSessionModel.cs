using AutoMapper;
using pingu.Data.Entities.Identity;
using pingu.Providers.JwtBearer;

namespace pingu.Models.Identity;

public class UserSessionModel : UserProfileModel
{
    public string TokenType { get; init; } = null!;
    public string AccessToken { get; init; } = null!;
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
    public string RefreshToken { get; init; } = null!;
    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
}

public class UserSessionModelProfile : Profile
{
    public UserSessionModelProfile()
    {
        CreateMap<User, UserSessionModel>();
        CreateMap<UserProfileModel, UserSessionModel>();
        CreateMap<JwtTokenInfo, UserSessionModel>();
    }
}