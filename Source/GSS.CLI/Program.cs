using GSS.CLI.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<CheckCommand>("check")
        .WithDescription("Checks if required tools are installed");

    config.AddCommand<NewCommand>("new")
        .WithDescription("Scaffolds a new GSS solution from a template");

    config.AddBranch("add", add =>
    {
        add.AddCommand<AddServiceCommand>("service")
            .WithDescription("Adds a new service to the solution");
    });
});

return app.Run(args);