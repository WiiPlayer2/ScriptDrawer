using ScriptDrawer.Core;
using ScriptDrawer.Serialization;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Cli;

internal class Tool
{
    private readonly ConfigSerializer configSerializer;

    private readonly Engine engine;

    public Tool(Engine engine, ConfigSerializer configSerializer)
    {
        this.engine = engine;
        this.configSerializer = configSerializer;
    }

    private async Task<object?> ParseConfiguration(FileInfo? configFile, Type configurationType)
    {
        if (configFile is null) return null;
        var content = await File.ReadAllTextAsync(configFile.FullName);
        var configuration = configSerializer.Deserialize(content, configurationType);
        return configuration;
    }

    public async Task Run(Input input)
    {
        var pipelineCode = await File.ReadAllTextAsync(input.PipelineFile.FullName);
        var pipeline = await engine.CompilePipelineAsync(pipelineCode, CancellationToken.None);
        var configuration = await ParseConfiguration(input.ConfigFile, pipeline.ConfigurationType);
        var publisher = new DelegatePublisher((name, image, cancellationToken) => SaveImage(input.OutputDirectory, name, image, cancellationToken));
        await pipeline.ExecuteAsync(publisher, configuration, CancellationToken.None);
    }

    private async Task SaveImage(DirectoryInfo outputDirectory, string name, Image image, CancellationToken cancellationToken)
    {
        outputDirectory.Create();
        var targetPath = Path.Combine(outputDirectory.FullName, $"{name}.png");
        await image.SaveAsPngAsync(targetPath, cancellationToken);
    }
}
