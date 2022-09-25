using System.CommandLine;
using ScriptDrawer.Core;
using SixLabors.ImageSharp;
using YamlDotNet.Serialization;

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

async Task Run(FileInfo pipelineFile, FileInfo? configFile, DirectoryInfo outputDirectory)
{
    var engine = new Engine();
    var pipelineCode = await File.ReadAllTextAsync(pipelineFile.FullName);
    var pipeline = await engine.CompilePipelineAsync(pipelineCode, CancellationToken.None);
    var configuration = await ParseConfiguration(configFile, pipeline.ConfigurationType);
    var publisher = new DelegatePublisher((name, image, cancellationToken) => SaveImage(outputDirectory, name, image, cancellationToken));
    await pipeline.ExecuteAsync(publisher, configuration, CancellationToken.None);
}

async Task SaveImage(DirectoryInfo outputDirectory, string name, Image image, CancellationToken cancellationToken)
{
    outputDirectory.Create();
    var targetPath = Path.Combine(outputDirectory.FullName, $"{name}.png");
    await image.SaveAsPngAsync(targetPath, cancellationToken);
}

async Task<object?> ParseConfiguration(FileInfo? configFile, Type configurationType)
{
    if (configFile is null) return null;
    var deserializer = new DeserializerBuilder().Build();
    var content = await File.ReadAllTextAsync(configFile.FullName);
    var configuration = deserializer.Deserialize(content, configurationType);
    return configuration;
}
