using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace ScriptDrawer.Serialization;

internal static class Helper
{
    public static IParser Parse(this YamlNode node) => node.ToParsingEvents().Parse();

    public static IParser Parse(this IEnumerable<ParsingEvent> events) => new NodeParser(events.GetEnumerator());

    public static IEnumerable<ParsingEvent> ToParsingEvents(this YamlNode node)
    {
        switch (node)
        {
            case YamlScalarNode scalarNode:
                yield return new Scalar(scalarNode.Anchor, scalarNode.Tag, scalarNode.Value ?? string.Empty, scalarNode.Style, default, default, scalarNode.Start, scalarNode.End);
                break;

            case YamlMappingNode mappingNode:
                yield return new MappingStart(mappingNode.Anchor, mappingNode.Tag, default, mappingNode.Style, mappingNode.Start, mappingNode.End);
                foreach (var (key, value) in mappingNode.Children)
                {
                    var keyScalar = (YamlScalarNode) key;
                    yield return new Scalar(keyScalar.Anchor, keyScalar.Tag, keyScalar.Value ?? string.Empty, keyScalar.Style, default, default, keyScalar.Start, keyScalar.End, true);

                    foreach (var @event in value.ToParsingEvents())
                        yield return @event;
                }

                yield return new MappingEnd(mappingNode.Start, mappingNode.End);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private class NodeParser : IParser
    {
        private readonly IEnumerator<ParsingEvent> enumerator;

        public NodeParser(IEnumerator<ParsingEvent> enumerator)
        {
            this.enumerator = enumerator;
        }

        public ParsingEvent? Current => enumerator.Current;

        public bool MoveNext() => enumerator.MoveNext();
    }
}
