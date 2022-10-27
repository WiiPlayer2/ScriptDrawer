using System;
using ScriptDrawer.Core.Refs;
using ScriptDrawer.Core.Refs.Mappers;
using YamlDotNet.Core;

namespace ScriptDrawer.Serialization.Deserializers;

internal class FileRefDeserializer<T, TMapper> : NodeDeserializer<FileRef<T, TMapper>, string>
    where TMapper : struct, IMapper<Stream, T>
{
    protected override FileRef<T, TMapper> Deserialize(string intermediateValue, Func<IParser, Type, object?> nestedObjectDeserializer) => new(intermediateValue);
}
