using System;
using ScriptDrawer.Core.Refs;
using ScriptDrawer.Core.Refs.Mappers;
using YamlDotNet.Core;

namespace ScriptDrawer.Serialization.Deserializers;

internal class UrlRefDeserializer<T, TMapper> : NodeDeserializer<UrlRef<T, TMapper>, Uri>
    where TMapper : struct, IMapper<Stream, T>
{
    protected override UrlRef<T, TMapper> Deserialize(Uri intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(intermediateValue);
}
