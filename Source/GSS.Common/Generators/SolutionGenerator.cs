using System.Text.Json;

namespace GSS.Common.Generators;

public class SolutionGenerator
{
    public void Generate(string name, string template, string outputDirectory)
    {
        var solutionDir = Path.Combine(outputDirectory, name);
        if (Directory.Exists(solutionDir))
        {
            throw new Exception($"Directory {solutionDir} already exists.");
        }

        Directory.CreateDirectory(solutionDir);
        
        // Create folders
        var srcDir = Path.Combine(solutionDir, "src");
        Directory.CreateDirectory(srcDir);
        Directory.CreateDirectory(Path.Combine(solutionDir, "docs"));

        // Create global.json
        File.WriteAllText(Path.Combine(solutionDir, "global.json"), 
            """
            {
              "sdk": {
                "version": "9.0.100",
                "rollForward": "latestFeature"
              }
            }
            """);

        // Create Directory.Build.props
        File.WriteAllText(Path.Combine(solutionDir, "Directory.Build.props"), 
            """
            <Project>
              <PropertyGroup>
                <TargetFramework>net9.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);
            
        // Create Directory.Packages.props
        File.WriteAllText(Path.Combine(solutionDir, "Directory.Packages.props"), 
            """
            <Project>
              <PropertyGroup>
                <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
              </PropertyGroup>
              <ItemGroup>
                <PackageVersion Include="Aspire.Hosting.AppHost" Version="9.0.0" />
              </ItemGroup>
            </Project>
            """);

        // Create gss.manifest.json
        var manifest = new
        {
            Name = name,
            Template = template,
            Services = new List<string>()
        };
        File.WriteAllText(Path.Combine(solutionDir, "gss.manifest.json"), 
            JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        // Run git init
        ProcessRunner.Run("git init", solutionDir);

        // Create AppHost
        var appHostName = $"{name}.AppHost";
        Console.WriteLine($"Creating {appHostName}...");
        // Ensure dotnet new works
        ProcessRunner.Run($"dotnet new aspire-apphost -n {appHostName} --output {appHostName}", srcDir);

        // Create SLNX
        var slnxPath = Path.Combine(solutionDir, $"{name}.slnx");
        var slnxContent = 
            $"""
            <Solution>
                <Folder Name="/docs/" />
                <Folder Name="/Solution Items/">
                    <File Path="Directory.Build.props" />
                    <File Path="Directory.Packages.props" />
                    <File Path="global.json" />
                    <File Path="gss.manifest.json" />
                </Folder>
                <Folder Name="/src/">
                    <Project Path="src/{appHostName}/{appHostName}.csproj" />
                </Folder>
            </Solution>
            """;
        File.WriteAllText(slnxPath, slnxContent);
    }
}

