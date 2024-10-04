namespace pingu.Providers.Ngrok;

public class NgrokOptions
{
    public bool ShowNgrokWindow { get; init; }

    public string AuthToken { get; set; } = null!;

    public string? Domain { get; init; } = null!;
}