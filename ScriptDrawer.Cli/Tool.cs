using Microsoft.Extensions.Logging;
using ScriptDrawer.Core;
using ScriptDrawer.Serialization;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Cli;

internal class Tool
{
    private readonly ConfigSerializer configSerializer;

    private readonly Engine engine;

    private readonly ILogger<Tool> logger;

    public Tool(Engine engine, ConfigSerializer configSerializer, ILogger<Tool> logger)
    {
        this.engine = engine;
        this.configSerializer = configSerializer;
        this.logger = logger;
    }

    private async Task<PipelineConfig?> ParseConfiguration(FileInfo? configFile, Type configurationType)
    {
        if (configFile is null) return null;

        logger.LogInformation("Reading configuration...");
        var content = await File.ReadAllTextAsync(configFile.FullName);
        var configuration = configSerializer.Deserialize(content, configurationType);
        return configuration;
    }

    public async Task Run(Input input)
    {
        logger.LogInformation("Compiling pipeline...");
        var pipelineCode = await File.ReadAllTextAsync(input.PipelineFile.FullName);
        var pipeline = await engine.CompilePipelineAsync(pipelineCode, CancellationToken.None);

        var configuration = await ParseConfiguration(input.ConfigFile, pipeline.ConfigurationType);

        logger.LogInformation("Executing pipeline...");
        var publisher = new DelegatePublisher((name, image, cancellationToken) => SaveImage(input.OutputDirectory, name, image, cancellationToken));
        await engine.RunPipelineAsync(pipeline, configuration, publisher, CancellationToken.None);
    }

    private async Task SaveImage(DirectoryInfo outputDirectory, string name, Image image, CancellationToken cancellationToken)
    {
        outputDirectory.Create();
        var targetPath = Path.Combine(outputDirectory.FullName, $"{name}.png");

        logger.LogInformation("Exporting {name} to {path}...", name, targetPath);
        await image.SaveAsPngAsync(targetPath, cancellationToken);
    }
}
