using ScriptDrawer.Core;
using ScriptDrawer.Core.Refs;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImagePipelineRefDeserializer : NodeDeserializer<ImagePipelineRef, ImagePipelineRefDeserializer.Config>
{
    private readonly Engine engine;

    public ImagePipelineRefDeserializer(Engine engine)
    {
        this.engine = engine;
    }

    protected override ImagePipelineRef Deserialize(Config intermediateValue) => new(Ref.To(async cancellationToken =>
    {
        var code = await intermediateValue.Instance.Pipeline.ResolveAsync(cancellationToken);
        var pipeline = await engine.CompilePipelineAsync(code, cancellationToken) ?? throw new InvalidOperationException();
        return new PipelineInstance(pipeline, default);
    }), intermediateValue.Publish);

    public class Config
    {
        public PipelineInstanceRaw Instance { get; init; } = default!;

        public string Publish { get; init; } = default!;

        public class PipelineInstanceRaw
        {
            public object? Configuration { get; init; }

            public IRef<string> Pipeline { get; init; } = default!;
        }
    }
}
