using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScriptDrawer.Cli;

return await BuildCommandLine()
    .UseHost(
        _ => Host.CreateDefaultBuilder(),
        host => host
            .ConfigureServices(services =>
            {
                services.AddScriptDrawer();
                services.AddSingleton<Tool>();
            }))
    .UseDefaults()
    .Build()
    .InvokeAsync(args);

CommandLineBuilder BuildCommandLine()
{
    var rootCommand = new RootCommand("CLI tool to execute ScriptDrawer pipelines.")
    {
        new Option<FileInfo>(
            "--pipeline-file",
            "The pipeline file to use.")
        {
            IsRequired = true,
        },
        new Option<FileInfo?>(
            "--config-file",
            "The configuration file to use."),
        new Option<DirectoryInfo>(
            "--output-directory",
            () => new DirectoryInfo(Environment.CurrentDirectory),
            "The directory in which all published images are put."),
    };

    rootCommand.Handler = CommandHandler.Create<Input, IHost>(
        (input, host) => host.Services.GetRequiredService<Tool>().Run(input));

    return new CommandLineBuilder(rootCommand);
}

internal record Input(FileInfo PipelineFile, FileInfo? ConfigFile, DirectoryInfo OutputDirectory);
