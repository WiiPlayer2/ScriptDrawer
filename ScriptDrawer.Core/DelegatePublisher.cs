using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core;

public class DelegatePublisher : IPublisher
{
    private readonly Func<string, Image, Task> publish;

    public DelegatePublisher(Func<string, Image, Task> publish)
    {
        this.publish = publish;
    }

    public DelegatePublisher(Action<string, Image> publish)
        : this((name, image) =>
        {
            publish(name, image);
            return Task.CompletedTask;
        }) { }

    public Task PublishAsync(string name, Image image) => publish(name, image);
}
