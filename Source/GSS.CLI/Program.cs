using GSS.CLI.Commands;
using GSS.CLI.Commands.List;
using GSS.CLI.Commands.New;
using GSS.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddSingleton<GlobalState>();
services.AddSingleton<ConfigGenerator>();

var registrar = new GSS.CLI.TypeRegistrar(services);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.AddCommand<CheckCommand>("check")
        .WithDescription("Checks if required tools are installed");

    config.AddBranch("new", @new =>
    {
        @new.AddCommand<NewRootCommand>("root")
            .WithDescription("Create a repo root config file");

        @new.AddCommand<NewProjectCommand>("project")
            .WithDescription("Create a project config file");
    });

    config.AddBranch("list", list =>
    {
        list.AddCommand<ListProjectTypesCommand>("types")
            .WithDescription("List available project types");
    });
});

return app.Run(args);