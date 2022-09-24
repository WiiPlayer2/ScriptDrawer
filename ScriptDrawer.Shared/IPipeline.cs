namespace ScriptDrawer.Shared;

public interface IPipeline
{
    public Task ExecuteAsync(IPublisher publisher);
}
