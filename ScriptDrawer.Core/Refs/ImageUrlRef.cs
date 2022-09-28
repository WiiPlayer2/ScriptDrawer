using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core.Refs;

public class ImageUrlRef : IRef<Image>
{
    public ImageUrlRef(Uri url)
    {
        Url = url;
    }

    public Uri Url { get; }

    public async Task<Image> ResolveAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(Url, cancellationToken);
        return await Image.LoadAsync(stream, cancellationToken);
    }
}
