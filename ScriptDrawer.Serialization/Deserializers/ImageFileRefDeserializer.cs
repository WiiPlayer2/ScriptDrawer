using ScriptDrawer.Core.Refs;
using YamlDotNet.Core;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImageFileRefDeserializer : NodeDeserializer<ImageFileRef, string>
{
    protected override ImageFileRef Deserialize(string intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(intermediateValue);
}
