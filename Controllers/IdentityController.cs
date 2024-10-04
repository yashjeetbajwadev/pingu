using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using pingu.Data.Entities.Identity;
using pingu.Models.Identity;
using pingu.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace pingu.Controllers;

/// <summary>
/// Controller responsible for handling identity operations.
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class IdentityController(IIdentityService identityService, ILogger<IdentityController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <param name="form">The data required to create the new user account.</param>
    /// <returns>The result of the account creation operation.</returns>
    [HttpPost("create")]
    public async Task<Results<ValidationProblem, Ok>> CreateAccount([FromBody] CreateAccountForm form)
    {
        return await identityService.CreateAccountAsync(form);
    }

    /// <summary>
    /// Sends a confirmation code to the user for account verification.
    /// </summary>
    /// <param name="form">The data required to send the confirmation code.</param>
    /// <returns>The result of sending the confirmation code.</returns>
    [HttpPost("confirm/send-code")]
    public async Task<Results<ValidationProblem, Ok>> SendConfirmAccountCode([FromBody] SendConfirmAccountCodeForm form)
    {
        return await identityService.SendConfirmAccountCodeAsync(form);
    }

    /// <summary>
    /// Confirms an existing user account using a confirmation code.
    /// </summary>
    /// <param name="form">The data required to confirm the account using the confirmation code.</param>
    /// <returns>The result of confirming the user account.</returns>
    [HttpPost("confirm")]
    public async Task<Results<ValidationProblem, Ok>> ConfirmAccount([FromBody] ConfirmAccountForm form)
    {
        return await identityService.ConfirmAccountAsync(form);
    }

    /// <summary>
    /// Requests a code to change user account details.
    /// </summary>
    /// <param name="form">The data required to request the code for changing account details.</param>
    /// <returns>The result of requesting the change code.</returns>
    [Authorize]
    [HttpPost("change/send-code")]
    public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> SendChangeAccountCode(
        [FromBody] SendChangeAccountCodeForm form)
    {
        return await identityService.SendChangeAccountCodeAsync(form);
    }

    /// <summary>
    /// Changes the current user account details using a confirmation code.
    /// </summary>
    /// <param name="form">The data required to change the account details using the confirmation code.</param>
    /// <returns>The result of changing the account details.</returns>
    [Authorize]
    [HttpPost("change")]
    public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangeAccount(
        [FromBody] ChangeAccountForm form)
    {
        return await identityService.ChangeAccountAsync(form);
    }

    /// <summary>
    /// Changes the password for the current user account.
    /// </summary>
    /// <param name="form">The data required to change the password.</param>
    /// <returns>The result of changing the password.</returns>
    [Authorize]
    [HttpPost("password/change")]
    public async Task<Results<ValidationProblem, UnauthorizedHttpResult, Ok>> ChangePassword(
        [FromBody] ChangePasswordForm form)
    {
        return await identityService.ChangePasswordAsync(form);
    }

    /// <summary>
    /// Sends a code to reset the user account password.
    /// </summary>
    /// <param name="form">The data required to request the password reset code.</param>
    /// <returns>The result of sending the reset code.</returns>
    [HttpPost("password/reset/send-code")]
    public async Task<Results<ValidationProblem, Ok>> ResetPassword([FromBody] SendResetPasswordCodeForm form)
    {
        return await identityService.SendResetPasswordCodeAsync(form);
    }

    /// <summary>
    /// Resets the password for the user account using a confirmation code.
    /// </summary>
    /// <param name="form">The data required to reset the password using the confirmation code.</param>
    /// <returns>The result of resetting the password.</returns>
    [HttpPost("password/reset")]
    public async Task<Results<ValidationProblem, Ok>> ResetPassword([FromBody] ResetPasswordForm form)
    {
        return await identityService.ResetPasswordAsync(form);
    }

    /// <summary>
    /// Signs into an existing user account.
    /// </summary>
    /// <param name="form">The data required to sign into the user account.</param>
    /// <returns>The result of the sign-in operation.</returns>
    [HttpPost("sign-in")]
    public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignIn([FromBody] SignInForm form)
    {
        return await identityService.SignInAsync(form);
    }

    /// <summary>
    /// Redirects the user to the external sign-in provider.
    /// </summary>
    /// <param name="signInManager">The sign-in manager service.</param>
    /// <param name="provider">The name of the external sign-in provider.</param>
    /// <param name="callbackUrl">The URL to redirect to after sign-in.</param>
    /// <returns>The result of the redirect operation.</returns>
    [HttpGet("sign-in/{provider}")]
    public Results<ValidationProblem, ChallengeHttpResult, Ok> SignInWithRedirect(
        [FromServices] SignInManager<User> signInManager, SignInWithProvider provider, [FromQuery] string callbackUrl)
    {
        var redirectUrl = Url.ActionLink(nameof(SignInWithCallback), values: new { provider, callbackUrl });
        var authenticationProperties =
            signInManager.ConfigureExternalAuthenticationProperties(provider.ToString(), redirectUrl: redirectUrl);
        return TypedResults.Challenge(authenticationProperties, [provider.ToString()]);
    }

    /// <summary>
    /// Handles the callback from an external sign-in provider after the user has authenticated.
    /// </summary>
    /// <param name="signInManager">The sign-in manager service used to retrieve external login information.</param>
    /// <param name="provider">The name of the external sign-in provider used for authentication.</param>
    /// <param name="callbackUrl">The URL to redirect the user to after processing the callback.</param>
    /// <returns>A redirect response that directs the user to the specified callback URL with a token for further processing.</returns>
    /// <remarks>
    /// This method processes the external login information received from the sign-in provider. If authentication information is successfully retrieved, it generates a token to protect user information and appends it to the callback URL. If the authentication result is null, the user is redirected to the original callback URL without a token.
    /// </remarks>
    [SwaggerIgnore]
    [HttpGet("sign-in/{provider}/callback")]
    public async Task<Results<RedirectHttpResult, Ok>> SignInWithCallback(
        [FromServices] SignInManager<User> signInManager, SignInWithProvider provider, [FromQuery] string callbackUrl)
    {
        var authenticationResult = await signInManager.GetExternalLoginInfoAsync();

        if (authenticationResult is null)
        {
            return TypedResults.Redirect(callbackUrl, permanent: true);
        }

        var protectedForm = await identityService.ProtectFormAsync(new SignInWithProviderForm
        {
            Provider = provider,
            ProviderKey = authenticationResult.ProviderKey,
            FirstName = authenticationResult.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "User",
            LastName = authenticationResult.Principal.FindFirstValue(ClaimTypes.Surname),
            Username = (authenticationResult.Principal.FindFirstValue(ClaimTypes.Email) ??
                        authenticationResult.Principal.FindFirstValue(ClaimTypes.MobilePhone) ??
                        authenticationResult.Principal.FindFirstValue(ClaimTypes.OtherPhone) ??
                        authenticationResult.Principal.FindFirstValue(ClaimTypes.HomePhone))!
        });

        callbackUrl = QueryHelpers.AddQueryString(callbackUrl,
            new Dictionary<string, StringValues>
            {
                { "provider", provider.ToString() },
                { "token", protectedForm },
                { "requestId", Guid.NewGuid().ToString("N") }
            });

        return TypedResults.Redirect(callbackUrl);
    }

    /// <summary>
    /// Signs in with an external provider using a token.
    /// </summary>
    /// <param name="provider">The external sign-in provider.</param>
    /// <param name="token">The token to use for sign-in.</param>
    /// <returns>The result of the sign-in operation.</returns>
    [HttpPost("sign-in/{provider}/{token}")]
    public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> SignInWithToken(
        [FromRoute] SignInWithProvider provider, [FromRoute] string token)
    {
        SignInWithProviderForm? form;

        try
        {
            form = await identityService.UnprotectFormAsync<SignInWithProviderForm>(token);
        }
        catch (Exception ex)
        {
            // Include provider in error
            logger.LogError(ex, "Failed to unprotect form for provider {Provider}.", provider);
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>(),
                title: $"{provider} Authentication failed.");
        }

        return await identityService.SignInWithAsync(form);
    }

    /// <summary>
    /// Refreshes the current user's access token.
    /// </summary>
    /// <param name="form">The data required to refresh the access token.</param>
    /// <returns>The result of refreshing the access token.</returns>
    [HttpPost("refresh-token")]
    public async Task<Results<ValidationProblem, Ok<UserSessionModel>>> RefreshToken([FromBody] RefreshTokenForm form)
    {
        return await identityService.RefreshTokenAsync(form);
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="form">The data required to sign out the user.</param>
    /// <returns>The result of the sign-out operation.</returns>
    [Authorize]
    [HttpPost("sign-out")]
    public async Task<Results<ValidationProblem, Ok>> SignOut([FromBody] SignOutForm form)
    {
        return await identityService.SignOutAsync(form);
    }

    /// <summary>
    /// Retrieves the current user's profile.
    /// </summary>
    /// <returns>The result of retrieving the user's profile.</returns>
    [Authorize]
    [HttpGet("profile")]
    public async Task<Results<UnauthorizedHttpResult, Ok<UserProfileModel>>> GetUserProfile()
    {
        return await identityService.GetUserProfileAsync();
    }
}