using System;
using ScriptDrawer.Core.Refs.Mappers;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Core.Refs;

public class FileRef<T, TMapper> : IRef<T>
    where TMapper : struct, IMapper<Stream, T>
{
    private TMapper mapper = default;

    public FileRef(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public async Task<T> ResolveAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(FilePath);
        return await mapper.MapAsync(stream, cancellationToken);
    }
}
