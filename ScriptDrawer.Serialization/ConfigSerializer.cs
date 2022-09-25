using YamlDotNet.Serialization;

namespace ScriptDrawer.Serialization;

public class ConfigSerializer
{
    private readonly IDeserializer deserializer;

    public ConfigSerializer()
    {
        deserializer = new DeserializerBuilder()
            .Build();
    }

    public object? Deserialize(string? content, Type configurationType)
        => content is null
            ? null
            : deserializer.Deserialize(content, configurationType);
}
