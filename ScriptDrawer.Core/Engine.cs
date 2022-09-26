using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Core;

public class Engine
{
    public Task<IPipeline> CompilePipelineAsync(string code, CancellationToken cancellationToken)
        => CompilePipelineAsync(code, type => (IPipeline) Activator.CreateInstance(type)!, cancellationToken);

    public async Task<IPipeline> CompilePipelineAsync(string code, Func<Type, IPipeline> instantiate, CancellationToken cancellationToken)
    {
        var scriptOptions = ScriptOptions.Default
            .WithMetadataResolver(EngineMetadataResolver.Default);
        var script = CSharpScript.Create<Type>(code, scriptOptions);
        var pipelineType = (await script.RunAsync(cancellationToken: cancellationToken)).ReturnValue;
        if (pipelineType is null)
            throw new InvalidOperationException("No pipeline type was returned.");
        return instantiate(pipelineType);
    }

    public Task RunPipelineAsync(IPipeline pipeline, PipelineConfig? config, IPublisher publisher, CancellationToken cancellationToken)
    {
        return RunPipelineAsync(pipeline, config, CreatePublisher, cancellationToken);

        IPublisher CreatePublisher(IReadOnlyList<int> indices, object? config)
            => new DelegatePublisher((name, image, cancellationToken) => publisher.PublishAsync(MapName(indices, name), image, cancellationToken));

        string MapName(IReadOnlyList<int> indices, string name)
            => indices.Count == 0
                ? name
                : $"{string.Join("-", indices)}_{name}";
    }

    public async Task RunPipelineAsync(IPipeline pipeline, PipelineConfig? config, Func<IReadOnlyList<int>, object?, IPublisher> createPublisher, CancellationToken cancellationToken)
    {
        if (config is null)
            await pipeline.ExecuteAsync(createPublisher(Array.Empty<int>(), default), default, cancellationToken);
        else
            foreach (var (indices, configuration) in config.BuildConfigs())
                await pipeline.ExecuteAsync(createPublisher(indices, configuration), configuration, cancellationToken);
    }
}
