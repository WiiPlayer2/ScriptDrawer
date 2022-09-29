using ScriptDrawer.Shared;

namespace ScriptDrawer.Core.Refs;

public static class Ref
{
    public static IRef<T> To<T>(Func<CancellationToken, Task<T>> resolver) => new DelegateRef<T>(resolver);

    private class DelegateRef<T> : IRef<T>
    {
        private readonly Func<CancellationToken, Task<T>> resolver;

        public DelegateRef(Func<CancellationToken, Task<T>> resolver)
        {
            this.resolver = resolver;
        }

        public Task<T> ResolveAsync(CancellationToken cancellationToken) => resolver(cancellationToken);
    }
}
