using ScriptDrawer.Core;

namespace ScriptDrawer.Serialization;

internal class ImageFileRefDeserializer : NodeDeserializer<ImageFileRef, string>
{
    protected override ImageFileRef Deserialize(string intermediateValue) => new(intermediateValue);
}
