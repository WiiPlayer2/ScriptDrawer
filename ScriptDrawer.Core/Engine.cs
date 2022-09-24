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
}
