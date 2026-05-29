using System.Text.Json;
using System.Text.Json.Serialization;

using GSS.CLI.Configuration;

namespace GSS.CLI.Services;

public sealed class GlobalState
{
    private const string RootConfigPattern = "gss.root.*.json";
    private const string ProjectConfigPattern = "gss.project.*.json";

    public GlobalState()
    {
        CurrentDirectory = Directory.GetCurrentDirectory();
        RootConfigDirectory = DetectRootConfigDirectory(CurrentDirectory);
        ActiveProjectDirectory = DetectActiveProjectDirectory(CurrentDirectory);
    }

    public string CurrentDirectory { get; }

    public string? RootConfigDirectory { get; private set; }

    public string? ActiveProjectDirectory { get; }

    public bool IsRootActive => RootConfigDirectory is not null;

    public bool IsProjectActive => ActiveProjectDirectory is not null;

    public RootConfig? RootConfig { get; private set; }

    public ProjectConfig? ProjectConfig { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (RootConfigDirectory is not null)
        {
            var rootConfigPath = FindMatchingFilePath(RootConfigDirectory, RootConfigPattern);
            if (rootConfigPath is not null)
            {
                var rootJson = await File.ReadAllTextAsync(rootConfigPath, cancellationToken);
                RootConfig = JsonSerializer.Deserialize<RootConfig>(rootJson, SerializerOptions);
            }
        }

        if (ActiveProjectDirectory is not null)
        {
            var projectConfigPath = FindMatchingFilePath(ActiveProjectDirectory, ProjectConfigPattern);
            if (projectConfigPath is not null)
            {
                var projectJson = await File.ReadAllTextAsync(projectConfigPath, cancellationToken);
                ProjectConfig = JsonSerializer.Deserialize<ProjectConfig>(projectJson, SerializerOptions);
            }
        }
    }

    public static string? DetectRootConfigDirectory(string startDirectory)
    {
        return FindNearestDirectory(startDirectory, directory => directory.EnumerateFiles(RootConfigPattern).Any());
    }

    public static string? DetectActiveProjectDirectory(string startDirectory)
    {
        return FindNearestDirectory(startDirectory, directory => directory.EnumerateFiles(ProjectConfigPattern).Any());
    }

    private static string? FindMatchingFilePath(string startDirectory, string searchPattern)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);

        while (currentDirectory is not null)
        {
            var matchingFile = currentDirectory.EnumerateFiles(searchPattern).FirstOrDefault();
            if (matchingFile is not null)
            {
                return matchingFile.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static string? FindNearestDirectory(string startDirectory, Func<DirectoryInfo, bool> predicate)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);

        while (currentDirectory is not null)
        {
            if (predicate(currentDirectory))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}