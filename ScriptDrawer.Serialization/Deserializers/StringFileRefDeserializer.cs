using ScriptDrawer.Core.Refs;

namespace ScriptDrawer.Serialization.Deserializers;

internal class StringFileRefDeserializer : NodeDeserializer<StringFileRef, string>
{
    protected override StringFileRef Deserialize(string intermediateValue) => new(intermediateValue);
}
