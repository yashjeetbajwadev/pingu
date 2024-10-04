namespace pingu.Providers.Messaging;

public class Message
{
    public string? AccountId { get; set; } = null!;

    public string? Subject { get; init; } = null!;

    public string? Body { get; init; }

    public string[] Recipients { get; init; } = [];
}