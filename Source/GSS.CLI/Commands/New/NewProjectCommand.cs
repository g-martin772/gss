using GSS.CLI.Services;
using GSS.CLI.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GSS.CLI.Commands.New;

public sealed class NewProjectCommand(ConfigGenerator generator, GlobalState state) : Command<NewProjectCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<PATH>")]
        public string Path { get; init; } = string.Empty;

        [CommandOption("-t|--type <TYPE>")]
        public ProjectType Type { get; init; } = ProjectType.Library;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!state.IsRootActive)
        {
            AnsiConsole.MarkupLine("[red]No active root was detected.[/] Create a root config before creating a project.");
            return -1;
        }

        if (state.IsProjectActive)
        {
            AnsiConsole.MarkupLine("[red]A project is already active.[/] Leave the project before creating another one.");
            return -1;
        }

        try
        {
            var filePath = generator.CreateProjectConfig(settings.Path, settings.Type);
            AnsiConsole.MarkupLine($"[green]Created project config:[/] {filePath}");
            return 0;
        }
        catch (Exception exception)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{exception.Message}[/]");
            return -1;
        }
    }
}