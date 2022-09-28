using ScriptDrawer.Core.Refs;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImageUrlRefDeserializer : NodeDeserializer<ImageUrlRef, Uri>
{
    protected override ImageUrlRef Deserialize(Uri intermediateValue) => new(intermediateValue);
}
