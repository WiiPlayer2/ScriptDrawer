using ScriptDrawer.Core.Refs;

namespace ScriptDrawer.Serialization.Deserializers;

internal class ImageFileRefDeserializer : NodeDeserializer<ImageFileRef, string>
{
    protected override ImageFileRef Deserialize(string intermediateValue) => new(intermediateValue);
}
