using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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

internal abstract class NodeDeserializer<T, TIntermediate> : NodeDeserializer<T>
{
    protected sealed override T Deserialize(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer)
    {
        var proxyParser = new ProxyParser(reader);
        return Deserialize((TIntermediate) nestedObjectDeserializer(proxyParser, typeof(TIntermediate))!);
    }

    protected abstract T Deserialize(TIntermediate intermediateValue);

    private class ProxyParser : IParser
    {
        private readonly IParser baseReader;

        public ProxyParser(IParser baseReader)
        {
            this.baseReader = baseReader;
            Current = WithoutTag(baseReader.Current);
        }

        public ParsingEvent? Current { get; private set; }

        public bool MoveNext()
        {
            if (!baseReader.MoveNext())
                return false;

            Current = baseReader.Current;
            return true;
        }

        private ParsingEvent? WithoutTag(ParsingEvent? @event)
        {
            if (@event is null or not NodeEvent)
                return @event;

            return @event switch
            {
                Scalar scalar => new Scalar(scalar.Anchor, default, scalar.Value, scalar.Style, scalar.IsPlainImplicit, scalar.IsQuotedImplicit),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
