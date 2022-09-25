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
            .ConfigureServices(ConfigureServices))
    .UseDefaults()
    .Build()
    .InvokeAsync(args);

void ConfigureServices(IServiceCollection services)
{
    services.AddScriptDrawer();
    services.AddSingleton<Tool>();
}

CommandLineBuilder BuildCommandLine()
{
    var pipelineFileOption = new Option<FileInfo>(
        "--pipeline-file",
        "The pipeline file to use.")
    {
        IsRequired = true,
    };

    var configFileOption = new Option<FileInfo?>(
        "--config-file",
        "The configuration file to use.");

    var outputDirectoryOption = new Option<DirectoryInfo>(
        "--output-directory",
        () => new DirectoryInfo(Environment.CurrentDirectory),
        "The directory in which all published images are put.");

    var rootCommand = new RootCommand("CLI tool to execute ScriptDrawer pipelines.")
    {
        pipelineFileOption,
        configFileOption,
        outputDirectoryOption,
    };

    rootCommand.Handler = CommandHandler.Create<Input, IHost>(
        (input, host) => host.Services.GetRequiredService<Tool>().Run(input.PipelineFile, input.ConfigFile, input.OutputDirectory));

    return new CommandLineBuilder(rootCommand);
}

internal record Input(FileInfo PipelineFile, FileInfo? ConfigFile, DirectoryInfo OutputDirectory);
