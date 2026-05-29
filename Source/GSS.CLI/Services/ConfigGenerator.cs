using System.Text.Json;
using System.Text.Json.Serialization;

using GSS.CLI.Configuration;

namespace GSS.CLI.Services;

public sealed class ConfigGenerator(GlobalState state)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string CreateRootConfig(string name)
    {
        if (state.IsRootActive)
        {
            throw new InvalidOperationException("A root is already active. Create projects inside the active root instead.");
        }

        if (state.IsProjectActive)
        {
            throw new InvalidOperationException("A project is already active. Leave the project before creating a root config.");
        }

        var filePath = Path.Combine(state.CurrentDirectory, $"gss.root.{SanitizeName(name)}.json");

        if (File.Exists(filePath))
        {
            throw new IOException($"Config already exists: {filePath}");
        }

        var config = new RootConfig
        {
            Name = name,
            VersionNumber = "1.0.0",
            Projects = []
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(config, SerializerOptions));
        return filePath;
    }

    public string CreateProjectConfig(string relativePath, ProjectType type)
    {
        if (!state.IsRootActive || state.RootConfigDirectory is null)
        {
            throw new InvalidOperationException("No active root was detected. Create a root config before creating a project.");
        }

        if (state.IsProjectActive)
        {
            throw new InvalidOperationException("A project is already active. Leave that project before creating another one.");
        }

        var rootConfigPath = FindMatchingFilePath(state.RootConfigDirectory, "gss.root.*.json")
            ?? throw new FileNotFoundException("The active root config file could not be found.");

        var rootDirectory = Path.GetDirectoryName(rootConfigPath)
            ?? throw new InvalidOperationException("The active root config is missing a directory.");

        var normalizedRelativePath = NormalizeRelativePath(relativePath);
        var projectDirectory = Path.GetFullPath(Path.Combine(new[] { rootDirectory }.Concat(normalizedRelativePath.Split('/')).ToArray()));

        if (!projectDirectory.StartsWith(Path.GetFullPath(rootDirectory), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The project path must stay inside the active root.");
        }

        Directory.CreateDirectory(projectDirectory);

        var projectName = Path.GetFileName(projectDirectory);
        var filePath = Path.Combine(projectDirectory, $"gss.project.{SanitizeName(projectName)}.json");

        if (File.Exists(filePath))
        {
            throw new IOException($"Config already exists: {filePath}");
        }

        var config = new ProjectConfig
        {
            Name = projectName,
            VersionNumber = "1.0.0"
        };

        var rootConfig = JsonSerializer.Deserialize<RootConfig>(File.ReadAllText(rootConfigPath), SerializerOptions)
            ?? throw new InvalidOperationException("The active root config could not be loaded.");

        if (rootConfig.Projects.Any(project => string.Equals(project.Path, normalizedRelativePath, StringComparison.OrdinalIgnoreCase)))
        {
            throw new IOException($"A project already exists at path: {normalizedRelativePath}");
        }

        File.WriteAllText(filePath, JsonSerializer.Serialize(config, SerializerOptions));

        rootConfig.Projects.Add(new RootProjectReference
        {
            Path = normalizedRelativePath,
            Type = type
        });

        File.WriteAllText(rootConfigPath, JsonSerializer.Serialize(rootConfig, SerializerOptions));
        return filePath;
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        var segments = relativePath
            .Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => segment.Trim())
            .ToArray();

        if (segments.Length == 0)
        {
            throw new ArgumentException("The project path cannot be empty.", nameof(relativePath));
        }

        if (segments.Any(segment => segment == "." || segment == ".."))
        {
            throw new ArgumentException("The project path cannot contain path traversal segments.", nameof(relativePath));
        }

        return string.Join('/', segments);
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

    private static string SanitizeName(string name)
    {
        var sanitizedCharacters = name.Select(character => Path.GetInvalidFileNameChars().Contains(character) || char.IsWhiteSpace(character) ? '-' : character).ToArray();
        return new string(sanitizedCharacters).Trim('-');
    }
}