using ScriptDrawer.Shared;

namespace ScriptDrawer.Core.Refs;

public class StringFileRef : IRef<string>
{
    public StringFileRef(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; }

    public Task<string> ResolveAsync(CancellationToken cancellationToken) => File.ReadAllTextAsync(FilePath, cancellationToken);
}
