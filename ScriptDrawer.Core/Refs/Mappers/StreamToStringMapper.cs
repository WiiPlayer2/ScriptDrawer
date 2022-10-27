using System;

namespace ScriptDrawer.Core.Refs.Mappers;

public struct StreamToStringMapper : IMapper<Stream, string>
{
    public async Task<string> MapAsync(Stream from, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(from);
        return await reader.ReadToEndAsync();
    }
}
