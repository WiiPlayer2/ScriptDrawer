﻿using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace ScriptDrawer.Serialization;

internal abstract class NodeDeserializer<T> : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (!typeof(T).IsAssignableFrom(expectedType))
        {
            value = default;
            return false;
        }

        value = Deserialize(reader, nestedObjectDeserializer);
        return true;
    }

    protected abstract T Deserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer);
}