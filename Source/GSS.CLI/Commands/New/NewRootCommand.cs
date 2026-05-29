using GSS.CLI.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GSS.CLI.Commands.New;

public sealed class NewRootCommand(ConfigGenerator generator, GlobalState state) : Command<NewRootCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<NAME>")]
        public string Name { get; init; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (state.IsRootActive)
        {
            AnsiConsole.MarkupLine("[red]A root is already active.[/] Use the existing root and create projects inside it.");
            return -1;
        }

        if (state.IsProjectActive)
        {
            AnsiConsole.MarkupLine("[red]A project is already active.[/] Leave the project before creating a root config.");
            return -1;
        }

        try
        {
            var filePath = generator.CreateRootConfig(settings.Name);
            AnsiConsole.MarkupLine($"[green]Created root config:[/] {filePath}");
            return 0;
        }
        catch (Exception exception)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{exception.Message}[/]");
            return -1;
        }
    }
}