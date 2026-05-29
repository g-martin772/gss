using System.Text.Json.Serialization;

namespace GSS.CLI.Configuration;

public sealed class ProjectConfig
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("versionNumber")]
    public string VersionNumber { get; init; } = "1.0.0";
}