using System.Text.Json.Serialization;

namespace GSS.CLI.Configuration;

public sealed class RootProjectReference
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public ProjectType Type { get; init; }
}