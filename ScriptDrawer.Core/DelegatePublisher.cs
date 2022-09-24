using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core;

public class DelegatePublisher : IPublisher
{
    private readonly Func<string, Image, CancellationToken, Task> publish;

    public DelegatePublisher(Func<string, Image, CancellationToken, Task> publish)
    {
        this.publish = publish;
    }

    public DelegatePublisher(Action<string, Image> publish)
        : this((name, image, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            publish(name, image);
            return Task.CompletedTask;
        }) { }

    public Task PublishAsync(string name, Image image, CancellationToken cancellationToken) => publish(name, image, cancellationToken);
}
