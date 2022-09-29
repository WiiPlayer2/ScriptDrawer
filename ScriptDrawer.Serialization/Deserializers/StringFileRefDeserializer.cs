using ScriptDrawer.Core.Refs;
using YamlDotNet.Core;

namespace ScriptDrawer.Serialization.Deserializers;

internal class StringFileRefDeserializer : NodeDeserializer<StringFileRef, string>
{
    protected override StringFileRef Deserialize(string intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(intermediateValue);
}
