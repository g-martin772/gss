using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using GSS.Common.Generators;

namespace GSS.CLI.Commands;

public class AddServiceCommand : Command<AddServiceCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<NAME>")]
        [Description("The name of the service")]
        public string Name { get; set; } = string.Empty;

        [CommandOption("-t|--template")]
        [Description("The template/type of service (e.g. REST)")]
        public string Template { get; set; } = "REST";
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"Adding service [green]{settings.Name}[/] using template [blue]{settings.Template}[/]...");

        try
        {
            var generator = new ServiceGenerator();
            // Assume current directory is the solution root for now
            generator.Generate(settings.Name, settings.Template, Directory.GetCurrentDirectory());
            AnsiConsole.MarkupLine("[green]Service added successfully![/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }

        return 0;
    }
}
