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

    public static DeserializerBuilder WithTaggedDeserializerMatrix(this DeserializerBuilder builder, Func<TaggedDeserializerMatrixBuilder, TaggedDeserializerMatrixBuilder> configure)
    {
        var (targetTypeTags, refTypeTags, mapperTypes) = configure(new TaggedDeserializerMatrixBuilder(new Dictionary<Type, string>(), new Dictionary<Type, (Type DeserializerType, string PartialTag)>(), new Dictionary<(Type FromType, Type ToType), Type>()));

        foreach (var (refType, (deserializerType, refPartialTag)) in refTypeTags)
        {
            var immediateType = refType
                .GetGenericArguments()
                .SelectMany(t => t.GetGenericParameterConstraints())
                .Single(t => t.IsInterface && t.GetGenericTypeDefinition() == typeof(IMapper<,>))
                .GenericTypeArguments[0];

            foreach (var (targetType, targetPartialTag) in targetTypeTags)
            {
                if (!mapperTypes.TryGetValue((immediateType, targetType), out var mapperType)) continue;

                var tag = $"!{targetPartialTag}{refPartialTag}";
                builder.WithTaggedDeserializer(tag, targetType, mapperType, refType, deserializerType);
            }
        }

        return builder;
    }

    public record TaggedDeserializerMatrixBuilder(
        Dictionary<Type, string> TargetTypeTags,
        Dictionary<Type, (Type DeserializerType, string PartialTag)> RefTypeTags,
        Dictionary<(Type FromType, Type ToType), Type> MapperTypes)
    {
        public TaggedDeserializerMatrixBuilder AddMapper<TFrom, TTo, TMapper>()
            where TMapper : struct, IMapper<TFrom, TTo>
        {
            MapperTypes.Add((typeof(TFrom), typeof(TTo)), typeof(TMapper));
            return this;
        }

        public TaggedDeserializerMatrixBuilder AddRefType(string partialTag, Type openRefType, Type openDeserializerType)
        {
            RefTypeTags.Add(openRefType, (openDeserializerType, partialTag));
            return this;
        }

        public TaggedDeserializerMatrixBuilder AddTargetType<T>(string partialTag)
        {
            TargetTypeTags.Add(typeof(T), partialTag);
            return this;
        }
    }
}
