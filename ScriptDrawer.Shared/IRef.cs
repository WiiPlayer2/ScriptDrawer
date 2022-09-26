namespace ScriptDrawer.Shared;

public interface IRef<T>
{
    Task<T> ResolveAsync(CancellationToken cancellationToken);
}
