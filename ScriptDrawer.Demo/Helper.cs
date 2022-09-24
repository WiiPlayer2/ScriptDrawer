using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Demo;

internal static class Helper
{
    public static Task ExecuteAsync(this IPipeline pipeline, IPublisher publisher, CancellationToken cancellationToken) => pipeline.ExecuteAsync(publisher, default, cancellationToken);

    public static IObservable<TResult> SelectAsync<T, TResult>(this IObservable<T> observable, Func<T, CancellationToken, Task<TResult>> selector)
        => observable
            .Select(item => Observable.FromAsync(cancellationToken => selector(item, cancellationToken)))
            .Switch();

    public static IDisposable Subscribe<T>(this IObservable<T> observable)
        => observable.Subscribe(_ => { });
}
