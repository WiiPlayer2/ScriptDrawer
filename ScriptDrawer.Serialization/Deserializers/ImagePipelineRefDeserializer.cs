using ScriptDrawer.Core;
using ScriptDrawer.Core.Refs;
using ScriptDrawer.Shared;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImagePipelineRefDeserializer : NodeDeserializer<ImagePipelineRef, ImagePipelineRefDeserializer.Config>
{
    private readonly Engine engine;

    public ImagePipelineRefDeserializer(Engine engine)
    {
        this.engine = engine;
    }

    protected override ImagePipelineRef Deserialize(Config intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(Ref.To(async cancellationToken =>
    {
        var code = await intermediateValue.Instance.Pipeline.ResolveAsync(cancellationToken);
        var pipeline = await engine.CompilePipelineAsync(code, cancellationToken) ?? throw new InvalidOperationException();
        var config = nestedObjectDeserializer(intermediateValue.Instance.Configuration.Parse(), pipeline.ConfigurationType);
        return new PipelineInstance(pipeline, config);
    }), intermediateValue.Publish);

    public class Config
    {
        public PipelineInstanceRaw Instance { get; init; } = default!;

        public string Publish { get; init; } = default!;

        public class PipelineInstanceRaw
        {
            public YamlMappingNode Configuration { get; init; } = default!;

            public IRef<string> Pipeline { get; init; } = default!;
        }
    }
}
