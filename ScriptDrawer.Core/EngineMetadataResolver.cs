using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace ScriptDrawer.Core;

internal class EngineMetadataResolver : MetadataReferenceResolver
{
    private readonly MetadataReferenceResolver baseResolver;

    private readonly Dictionary<string, string> defaultAssemblyLocations = new();

    private EngineMetadataResolver(MetadataReferenceResolver baseResolver)
    {
        this.baseResolver = baseResolver;

        Register(typeof(IPipeline).Assembly);
        Register(typeof(Image).Assembly);
        Register(typeof(RegularPolygon).Assembly);

        void Register(Assembly assembly) => defaultAssemblyLocations.Add(assembly.GetName().Name!, assembly.Location);
    }

    public static EngineMetadataResolver Default { get; } = new(ScriptMetadataResolver.Default);

    public override bool Equals(object? other) => throw new NotImplementedException();

    public override int GetHashCode() => throw new NotImplementedException();

    public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string? baseFilePath, MetadataReferenceProperties properties)
    {
        var matchingAssemblyLocations = defaultAssemblyLocations
            .Where(kv => reference.Contains(kv.Key, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        if (matchingAssemblyLocations.Count == 0)
            return baseResolver.ResolveReference(reference, baseFilePath, properties);

        var resolvedAssemblies = ImmutableArray.Create(MetadataReference.CreateFromFile(matchingAssemblyLocations.MaxBy(kv => kv.Key).Value));
        return resolvedAssemblies;
    }
}
