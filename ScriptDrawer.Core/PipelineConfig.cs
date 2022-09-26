using System.Collections.Immutable;
using System.Reflection;

namespace ScriptDrawer.Core;

public record PipelineConfig(Type Type, IReadOnlyDictionary<string, IReadOnlyList<object?>> Matrix, object Configuration)
{
    public IEnumerable<(IReadOnlyList<int> Indices, object Configuration)> BuildConfigs()
    {
        var indices = new int[Matrix.Count];
        var props = new PropertyInfo[Matrix.Count];
        var matrixPairs = Matrix.ToList();

        for (var i = 0; i < Matrix.Count; i++) props[i] = Type.GetProperty(matrixPairs[i].Key)!;

        while (true)
        {
            var config = CloneConfig();
            for (var i = 0; i < indices.Length; i++) props[i].SetValue(config, matrixPairs[i].Value[indices[i]]);

            yield return (indices.ToImmutableArray(), config);

            if (!IncrementIndices())
                break;
        }

        bool IncrementIndices()
        {
            for (var currentIndex = 0; currentIndex < indices.Length; currentIndex++)
            {
                indices[currentIndex]++;
                if (indices[currentIndex] == matrixPairs[currentIndex].Value.Count)
                    indices[currentIndex] = 0;
                else
                    return true;
            }

            return false;
        }

        object CloneConfig()
        {
            var copy = Activator.CreateInstance(Type)!;
            foreach (var property in Type.GetProperties())
                property.SetValue(copy, property.GetValue(Configuration));
            return copy;
        }
    }
}
