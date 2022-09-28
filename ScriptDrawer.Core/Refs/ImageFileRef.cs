using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core.Refs;

public class ImageFileRef : IRef<Image>
{
    public ImageFileRef(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public Task<Image> ResolveAsync(CancellationToken cancellationToken) => Image.LoadAsync(FilePath, cancellationToken);
}
