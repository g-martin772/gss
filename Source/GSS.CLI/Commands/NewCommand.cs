using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using GSS.Common.Generators;

namespace GSS.CLI.Commands;

public class NewCommand : Command<NewCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<TEMPLATE>")]
        [Description("The template to use (e.g. micro)")]
        public string Template { get; set; } = string.Empty;

        [CommandArgument(1, "[NAME]")]
        [Description("The name of the solution")]
        public string? Name { get; set; }
        
        [CommandOption("-o|--output")]
        [Description("The output directory")]
        public string? Output { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var name = settings.Name ?? "MySolution";
        var output = settings.Output ?? Directory.GetCurrentDirectory();
        
        AnsiConsole.MarkupLine($"Creating new solution [green]{name}[/] using template [blue]{settings.Template}[/]...");

        try 
        {
            var generator = new SolutionGenerator();
            generator.Generate(name, settings.Template, output);
            AnsiConsole.MarkupLine("[green]Solution created successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
        
        return 0;
    }
}

