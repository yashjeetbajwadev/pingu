using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Settings.Configuration;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using pingu.Data.Entities.Identity;
using pingu.Data;
using pingu.Providers.Messaging.MailKit;
using pingu.Middlewares;
using pingu.Extensions;
using pingu.Providers.SwaggerGen;
using pingu.Helpers;
using pingu.Providers.Messaging.Twilio;
using pingu.Providers.JwtBearer;
using pingu.Providers.RazorViewRender;
using pingu.Providers.ModelValidator;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using pingu.Options;
using pingu.Providers.Ngrok;
using pingu.Services;

try
{
    var defaultCultureInfo = new CultureInfo("en-NZ");
    CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;

    var appAssemblies = AssemblyHelper.GetAppAssemblies().ToArray();
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration, new ConfigurationReaderOptions { SectionName = "Serilog" })
        .Enrich.FromLogContext()
        .CreateLogger();

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog(Log.Logger);

    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        var serializerOptions = options.SerializerOptions;
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddDataProtection()
        .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
        {
            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
            ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = Helper.GetEnvironmentVariable("PINGU_DB_CONNECTION_STRING");
        options.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
    });

    builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.AllowedUserNameCharacters = string.Empty;
        options.User.RequireUniqueEmail = false;

        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;

        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
        options.ClaimsIdentity.SecurityStampClaimType = ClaimTypes.SerialNumber;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<User, Role>>();

    builder.Services.AddAutoMapper(appAssemblies);
    builder.Services.AddModelValidator(appAssemblies);
    builder.Services.AddRazorViewRenderer(appAssemblies);

    builder.Services.AddMailKitSender(options =>
    {
        builder.Configuration.GetRequiredSection("MailKit").Bind(options);
    });

    builder.Services.AddTwilioSender(options =>
    {
        builder.Configuration.GetRequiredSection("Twilio").Bind(options);
    });

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtProvider(options =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            if (allowedOrigins != null && allowedOrigins.Length != 0)
                options.Issuer = string.Join(";", allowedOrigins);
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = IdentityConstants.ExternalScheme;
            options.ClientId = Helper.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            options.ClientSecret = Helper.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
        });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy = policy
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition")
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                .SetIsOriginAllowedToAllowWildcardSubdomains();

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            if (allowedOrigins != null)
                policy.WithOrigins(allowedOrigins);
            else
                policy.AllowAnyOrigin();
        });
    });

    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = false;
    })
    .AddControllers()
    .AddJsonOptions(options =>
    {
        var serializerOptions = options.JsonSerializerOptions;
        serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        serializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
    builder.Services.AddSwaggerGen();

    builder.Services.Configure<IdentityServiceOptions>(options =>
    {
        options.FormProtectorKey = Guid.NewGuid().ToString("N");
    });
    builder.Services.AddScoped<IIdentityService, IdentityService>();

    if (builder.Environment.IsDevelopment())
    {
        var startNgrokService = builder.Configuration.GetValue<bool>("Ngrok:StartNgrokService");
        if (startNgrokService)
        {
            Log.Information("Registering Ngrok hosted service as the application is running in development.");
            builder.Services.AddNgrokHostedService(options =>
            {
                options.AuthToken = Helper.GetEnvironmentVariable("NGROK_AUTH_TOKEN");
            });
        }
        else
        {
            Log.Information("Skipping Ngrok hosted service registration as 'Ngrok:StartNgrokService' is not enabled.");
        }
    }

    var app = builder.Build();

    await app.RunDbMigrationsAsync<ApplicationDbContext>();
    app.UseDbTransaction<ApplicationDbContext>();

    app.UseStatusCodePagesWithReExecute("/errors/{0}");
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        AllowStatusCode404Response = true,
        ExceptionHandler = null,
        ExceptionHandlingPath = "/errors/500"
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}