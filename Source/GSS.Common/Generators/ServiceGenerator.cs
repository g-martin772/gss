using System.Text.Json;
using System.Text.Json.Nodes;

namespace GSS.Common.Generators;

public class ServiceGenerator
{
    public void Generate(string serviceName, string template, string solutionDirectory)
    {
         var manifestPath = Path.Combine(solutionDirectory, "gss.manifest.json");
         if (!File.Exists(manifestPath))
         {
             throw new Exception("Not a GSS solution directory (missing gss.manifest.json).");
         }
         
         // Finding SLNX file.
         var slnxFiles = Directory.GetFiles(solutionDirectory, "*.slnx");
         if (slnxFiles.Length == 0) throw new Exception("No .slnx file found.");
         var slnxPath = slnxFiles[0];
         var solutionName = Path.GetFileNameWithoutExtension(slnxPath);

         var srcDir = Path.Combine(solutionDirectory, "src");
         var serviceDir = Path.Combine(srcDir, serviceName);
         
         if (Directory.Exists(serviceDir)) throw new Exception($"Service {serviceName} already exists.");
         Directory.CreateDirectory(serviceDir);

         // Define project names
         var apiName = $"{serviceName}.Api";
         var contractsName = $"{serviceName}.Contracts";
         var domainName = $"{serviceName}.Domain";
         var infraName = $"{serviceName}.Infrastructure";

         // Create Projects
         Console.WriteLine("Creating projects...");
         ProcessRunner.Run($"dotnet new webapi -n {apiName} --output {apiName}", serviceDir);
         ProcessRunner.Run($"dotnet new classlib -n {contractsName} --output {contractsName}", serviceDir);
         ProcessRunner.Run($"dotnet new classlib -n {domainName} --output {domainName}", serviceDir);
         ProcessRunner.Run($"dotnet new classlib -n {infraName} --output {infraName}", serviceDir);

         // Add References
         Console.WriteLine("Linking projects...");
         // Infrastructure -> Domain
         ProcessRunner.Run($"dotnet add {infraName}/{infraName}.csproj reference ../{domainName}/{domainName}.csproj", serviceDir);
         // Api -> Contracts, Domain, Infrastructure
         ProcessRunner.Run($"dotnet add {apiName}/{apiName}.csproj reference ../{contractsName}/{contractsName}.csproj", serviceDir);
         ProcessRunner.Run($"dotnet add {apiName}/{apiName}.csproj reference ../{domainName}/{domainName}.csproj", serviceDir);
         ProcessRunner.Run($"dotnet add {apiName}/{apiName}.csproj reference ../{infraName}/{infraName}.csproj", serviceDir);
        
         // Add EF Core to Infrastructure (example package)
         Console.WriteLine("Adding NuGet packages...");
         try 
         {
             AddPackage(solutionDirectory, Path.Combine(serviceDir, infraName, $"{infraName}.csproj"), "Microsoft.EntityFrameworkCore.SqlServer", "9.0.0");
             AddPackage(solutionDirectory, Path.Combine(serviceDir, apiName, $"{apiName}.csproj"), "Microsoft.EntityFrameworkCore.Design", "9.0.0");
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Warning: Failed to add NuGet packages. {ex.Message}");
         }

         // Update SLNX
         Console.WriteLine("Updating solution...");
         UpdateSlnx(slnxPath, serviceName, apiName, contractsName, domainName, infraName);

         // Update AppHost
         var appHostName = $"{solutionName}.AppHost";
         var appHostDir = Path.Combine(srcDir, appHostName);
         if (Directory.Exists(appHostDir))
         {
             var appHostProj = Path.Combine(appHostDir, $"{appHostName}.csproj");
             if (File.Exists(appHostProj))
             {
                 Console.WriteLine("Adding reference to AppHost...");
                 ProcessRunner.Run($"dotnet add {appHostProj} reference ../{serviceName}/{apiName}/{apiName}.csproj", srcDir);
             }
         }
         
         // Update manifest
         UpdateManifest(manifestPath, serviceName);
    }
    
    private void AddPackage(string solutionHostDir, string projectPath, string packageName, string version)
    {
        // 1. Update Directory.Packages.props
        var propsPath = Path.Combine(solutionHostDir, "Directory.Packages.props");
        if (File.Exists(propsPath))
        {
            var propsContent = File.ReadAllText(propsPath);
            if (!propsContent.Contains($"Include=\"{packageName}\""))
            {
                 if (propsContent.Contains("</ItemGroup>"))
                 {
                     var newLine = $"    <PackageVersion Include=\"{packageName}\" Version=\"{version}\" />";
                     var lastItemGroupIndex = propsContent.LastIndexOf("</ItemGroup>", StringComparison.Ordinal);
                     if (lastItemGroupIndex != -1)
                     {
                        propsContent = propsContent.Insert(lastItemGroupIndex, newLine + Environment.NewLine + "  ");
                        File.WriteAllText(propsPath, propsContent);
                     }
                 }
            }
        }
        
        // 2. Add to Project
        var projContent = File.ReadAllText(projectPath);
        if (!projContent.Contains($"Include=\"{packageName}\""))
        {
             var packageRef = $"<PackageReference Include=\"{packageName}\" />";
             var newItemGroup = "\n  <ItemGroup>\n    " + packageRef + "\n  </ItemGroup>";
             
             if (projContent.Contains("</Project>"))

             {
                projContent = projContent.Replace("</Project>", newItemGroup + "\n</Project>");
                File.WriteAllText(projectPath, projContent);
             }
        }
    }

    private void UpdateSlnx(string slnxPath, string serviceName, params string[] projectNames)
    {
         var content = File.ReadAllText(slnxPath);
         
         var newBlock = $"\n    <Folder Name=\"/src/{serviceName}/\">";
         foreach(var p in projectNames)
         {
             newBlock += $"\n        <Project Path=\"src/{serviceName}/{p}/{p}.csproj\" />";
         }
         newBlock += "\n    </Folder>";
         
         var closingTag = "</Solution>";
         if (content.Contains(closingTag))
         {
             content = content.Replace(closingTag, newBlock + "\n" + closingTag);
             File.WriteAllText(slnxPath, content);
         }
    }
    
    private void UpdateManifest(string manifestPath, string serviceName)
    {
        var json = File.ReadAllText(manifestPath);
        var node = JsonNode.Parse(json);
        if (node != null)
        {
             var services = node["Services"]?.AsArray();
             
             // If Services property doesn't exist (case sensitivity issues?), create it?
             // Assuming it respects the casing from generation.
             if (services == null)
             {
                 // Handle if needed
                 var obj = node.AsObject();
                 services = new JsonArray();
                 obj["Services"] = services;
             }

             services.Add(serviceName);
             File.WriteAllText(manifestPath, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
