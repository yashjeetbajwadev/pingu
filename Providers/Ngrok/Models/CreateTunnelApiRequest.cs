﻿using System.Text.Json.Serialization;

namespace pingu.Providers.Ngrok.Models;

public class CreateTunnelApiRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("addr")]
    public string Address { get; set; } = null!;

    [JsonPropertyName("proto")]
    public string Protocol { get; set; } = null!;

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }
}