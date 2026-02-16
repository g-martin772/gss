using System.Diagnostics;

namespace GSS.Common;

public static class ProcessRunner
{
    public static void Run(string combinedCommand, string workingDirectory)
    {
        var parts = combinedCommand.Split(' ', 2);
        var fileName = parts[0];
        var args = parts.Length > 1 ? parts[1] : "";

        Run(fileName, args, workingDirectory);
    }

    public static void Run(string fileName, string args, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
        
        if (process?.ExitCode != 0)
        {
             var error = process?.StandardError.ReadToEnd();
             var output = process?.StandardOutput.ReadToEnd();
             throw new Exception($"Command '{fileName} {args}' failed.\nError: {error}\nOutput: {output}");
        }
    }
}

