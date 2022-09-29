using ScriptDrawer.Core.Refs;
using YamlDotNet.Core;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImageUrlRefDeserializer : NodeDeserializer<ImageUrlRef, Uri>
{
    protected override ImageUrlRef Deserialize(Uri intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(intermediateValue);
}
