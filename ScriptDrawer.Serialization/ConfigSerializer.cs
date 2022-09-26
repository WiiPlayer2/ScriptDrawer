using System.Diagnostics.CodeAnalysis;
using ScriptDrawer.Core;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace ScriptDrawer.Serialization;

public class ConfigSerializer
{
    private static DeserializerBuilder CreateBuilder() => new DeserializerBuilder()
        .WithTagMapping("!imageFile", typeof(ImageFileRef))
        .WithNodeDeserializer(new ImageFileRefDeserializer());

    public PipelineConfig? Deserialize(string? content, Type configurationType)
    {
        if (content is null) return null;

        var deserializer = CreateBuilder()
            .WithNodeDeserializer(new PipelineConfigDeserializer(configurationType))
            .Build();

        var config = deserializer.Deserialize<PCR?>(content);
        if (config is null) return null;

        var matrix = config.Matrix.Values
            .ToDictionary(o => o.Key, o => (IReadOnlyList<object?>) o.Value);
        var configuration = config.Configuration.Object;
        return new PipelineConfig(
            configurationType,
            matrix,
            configuration);
    }

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    private class PCR
    {
        public Config_ Configuration { get; init; } = default!;

        public Matrix_ Matrix { get; init; } = new(new Dictionary<string, List<object?>>());

        public class Config_
        {
            public Config_(object @object)
            {
                Object = @object;
            }

            public object Object { get; }
        }

        public class Matrix_
        {
            public Matrix_(Dictionary<string, List<object?>> values)
            {
                Values = values;
            }

            public Dictionary<string, List<object?>> Values { get; }
        }
    }

    private class PipelineConfigDeserializer : INodeDeserializer
    {
        private readonly Type configurationType;

        public PipelineConfigDeserializer(Type configurationType)
        {
            this.configurationType = configurationType;
        }

        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (expectedType == typeof(PCR.Matrix_))
            {
                value = ReadMatrix();
                return true;
            }

            if (expectedType == typeof(PCR.Config_))
            {
                value = new PCR.Config_(nestedObjectDeserializer(reader, configurationType) ?? throw new InvalidOperationException("Configuration may not be empty."));
                return true;
            }

            value = default;
            return false;

            PCR.Matrix_ ReadMatrix()
            {
                var dict = new Dictionary<string, List<object?>>();

                reader.Consume<MappingStart>();
                while (!reader.TryConsume<MappingEnd>(out _))
                {
                    var key = reader.Consume<Scalar>().Value;
                    var propertyInfo = configurationType.GetProperty(key) ?? throw new InvalidOperationException($"Property \"{key}\" not found.");
                    var propertyType = propertyInfo.PropertyType;
                    var values = ReadMatrixValues(propertyType);
                    dict.Add(key, values);
                }

                return new PCR.Matrix_(dict);
            }

            List<object?> ReadMatrixValues(Type type)
            {
                var values = new List<object?>();

                reader.Consume<SequenceStart>();
                while (!reader.TryConsume<SequenceEnd>(out _))
                {
                    var value = nestedObjectDeserializer(reader, type);
                    values.Add(value);
                }

                return values;
            }
        }
    }
}
