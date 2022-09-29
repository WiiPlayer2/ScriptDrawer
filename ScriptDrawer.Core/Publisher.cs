using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core;

public static class Publisher
{
    public static IPublisher For(Action<string, Image> publish) => new DelegatePublisher(publish);

    public static IPublisher For(Func<string, Image, CancellationToken, Task> publish) => new DelegatePublisher(publish);
}
