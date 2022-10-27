using System;
using ScriptDrawer.Core.Refs.Mappers;
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

    public static DeserializerBuilder WithTaggedDeserializer<T, TMapper>(this DeserializerBuilder builder, TagName tag, Type openRefType, Type openDeserializerType)
        where TMapper : struct, IMapper<Stream, T>
        => builder.WithTaggedDeserializer(tag, typeof(T), typeof(TMapper), openRefType, openDeserializerType);

    public static DeserializerBuilder WithTaggedDeserializer(this DeserializerBuilder builder, TagName tag, Type targetType, Type mapperType, Type openRefType, Type openDeserializerType)
        => builder
            .WithTagMapping(tag, openRefType.MakeGenericType(targetType, mapperType))
            .WithNodeDeserializer((INodeDeserializer) Activator.CreateInstance(openDeserializerType.MakeGenericType(targetType, mapperType))!);
}
