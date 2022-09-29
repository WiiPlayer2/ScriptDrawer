using ScriptDrawer.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ScriptDrawer.Core.Refs;

public class ImagePipelineRef : IRef<Image>
{
    public ImagePipelineRef(IRef<PipelineInstance> instance, string publishName)
    {
        Instance = instance;
        PublishName = publishName;
    }

    public IRef<PipelineInstance> Instance { get; }

    public string PublishName { get; }

    public async Task<Image> ResolveAsync(CancellationToken cancellationToken)
    {
        var instance = await Instance.ResolveAsync(cancellationToken);

        var result = default(Image?);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var publisher = Publisher.For((name, image) =>
        {
            if (name != PublishName) return;
            result = image.Clone(_ => { });
            cts.Cancel();
        });

        try
        {
            await instance.ExecuteAsync(publisher, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }

        if (result is null)
            throw new InvalidOperationException("No matching image published.");

        return result;
    }
}
