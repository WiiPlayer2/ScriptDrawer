using ScriptDrawer.Core;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace ScriptDrawer.Serialization;

internal class ImageFileRefDeserializer : NodeDeserializer<ImageFileRef>
{
    protected override ImageFileRef Deserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer) => new(reader.Consume<Scalar>().Value);
}
