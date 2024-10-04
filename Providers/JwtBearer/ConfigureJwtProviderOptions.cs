using DeviceId;
using Microsoft.Extensions.Options;
using pingu.Helpers;
using System.Reflection;

namespace pingu.Providers.JwtBearer;

public class ConfigureJwtProviderOptions(IHttpContextAccessor httpContextAccessor)
    : IConfigureOptions<JwtProviderOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public void Configure(JwtProviderOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        options.Secret ??= HashHelper.GenerateSHA256Hash(new DeviceIdBuilder()
            .AddMachineName()
            .AddOsVersion()
            .AddUserName()
            .AddFileToken(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, $"jwt-secret.txt")).ToString());

        var httpContext = (_httpContextAccessor?.HttpContext) ?? throw new InvalidOperationException("Unable to determine the current HttpContext.");
        var currentOrigin = string.Concat(httpContext.Request.Scheme, "://", httpContext.Request.Host.ToUriComponent()).ToLower();

        options.Issuer ??= currentOrigin;
        options.Audience ??= currentOrigin;
    }
}