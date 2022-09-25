using System.CommandLine;

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

rootCommand.SetHandler(Run, pipelineFileOption, configFileOption, outputDirectoryOption);
return await rootCommand.InvokeAsync(args);

Task Run(FileInfo pipelineFile, FileInfo? configFile, DirectoryInfo outputDirectory) => throw new NotImplementedException();
