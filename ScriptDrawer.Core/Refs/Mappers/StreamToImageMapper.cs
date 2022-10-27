using System;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core.Refs.Mappers;

public struct StreamToImageMapper : IMapper<Stream, Image>
{
    public Task<Image> MapAsync(Stream from, CancellationToken cancellationToken) => Image.LoadAsync(from, cancellationToken);
}
