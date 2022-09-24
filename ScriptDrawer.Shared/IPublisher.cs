using SixLabors.ImageSharp;

namespace ScriptDrawer.Shared;

public interface IPublisher
{
    public Task PublishAsync(string name, Image image);
}
