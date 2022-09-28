using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ScriptDrawer.Serialization;

internal static class DeserializerBuilderExtensions
{
    public static DeserializerBuilder WithTaggedDeserializer<T, TDeserializer>(this DeserializerBuilder builder, TagName tag)
        where TDeserializer : INodeDeserializer, new()
        => builder
            .WithTagMapping(tag, typeof(T))
            .WithNodeDeserializer(new TDeserializer());
}
