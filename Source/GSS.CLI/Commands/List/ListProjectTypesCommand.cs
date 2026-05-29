using GSS.CLI.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GSS.CLI.Commands.List;

public sealed class ListProjectTypesCommand : Command<ListProjectTypesCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var table = new Table()
            .Border(TableBorder.Rounded);

        table.AddColumn("[bold]Type[/]");
        table.AddColumn("[bold]Value[/]");

        foreach (var projectType in Enum.GetValues<ProjectType>())
        {
            table.AddRow(projectType.ToString(), projectType.ToString().ToLowerInvariant());
        }

        AnsiConsole.Write(table);
        return 0;
    }
}