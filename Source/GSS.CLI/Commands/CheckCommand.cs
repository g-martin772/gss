using System.Diagnostics;

using Spectre.Console;
using Spectre.Console.Cli;

namespace GSS.CLI.Commands;

public class CheckCommand : Command<CheckCommand.Settings>
{
    public class Settings : CommandSettings;

    private record ToolCheck(string Name, string VersionFlag = "--version", string? MinVersion = null);

    private static readonly ToolCheck[] Tools =
    [
        new("git", MinVersion: "2.53"),
        new("dotnet", MinVersion: "10.0"),
        new("dotnet-ef", MinVersion: "10.0"),
        new("docker", MinVersion: "29.0"),
        new("aspire", MinVersion: "13.0"),
        new("kubectl", "version --client", "1.35"),
        new("helm", "version", "3.19"),
        new("node", MinVersion: "25.0"),
        new("npm", MinVersion: "11.0"),
        new("python", MinVersion: "3.14"),
        new("pip", MinVersion: "26.0")
    ];

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        foreach (var tool in Tools) CheckTool(tool);

        return 0;
    }

    private void CheckTool(ToolCheck tool)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = tool.Name,
                Arguments = tool.VersionFlag,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                PrintMissing(tool.Name);
                return;
            }

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                var version = output.Split('\n').FirstOrDefault() ?? "Unknown version";
                
                if (!string.IsNullOrEmpty(tool.MinVersion))
                {
                    var currentVersion = ExtractVersion(version);
                    if (currentVersion != null && CompareVersions(currentVersion, tool.MinVersion) < 0)
                    {
                        AnsiConsole.MarkupLine($"[yellow]{tool.Name}[/]: {version} [yellow](minimum: {tool.MinVersion})[/]");
                        return;
                    }
                }
                
                AnsiConsole.MarkupLine($"[green]{tool.Name}[/]: {version}");
            }
            else
            {
                PrintMissing(tool.Name);
            }
        }
        catch (Exception)
        {
            PrintMissing(tool.Name);
        }
    }

    private static string? ExtractVersion(string versionOutput)
    {
        var match = System.Text.RegularExpressions.Regex.Match(versionOutput, @"\d+\.\d+(?:\.\d+)?");
        return match.Success ? match.Value : null;
    }

    private static int CompareVersions(string version1, string version2)
    {
        var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
        var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
        
        var maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
        
        for (int i = 0; i < maxLength; i++)
        {
            var v1 = i < v1Parts.Length ? v1Parts[i] : 0;
            var v2 = i < v2Parts.Length ? v2Parts[i] : 0;
            
            if (v1 != v2) return v1.CompareTo(v2);
        }
        
        return 0;
    }

    private void PrintMissing(string toolName)
    {
        AnsiConsole.MarkupLine($"[red]{toolName}[/]: [red]Missing or not found[/]");
    }
}