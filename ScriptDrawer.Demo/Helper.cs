using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ScriptDrawer.Shared;

namespace ScriptDrawer.Demo;

internal static class Helper
{
    public static Task ExecuteAsync(this IPipeline pipeline, IPublisher publisher) => pipeline.ExecuteAsync(publisher, default);

    public static IObservable<TResult> SelectAsync<T, TResult>(this IObservable<T> observable, Func<T, Task<TResult>> selector)
        => observable
            .Select(item => Observable.FromAsync(() => selector(item)))
            .Merge();
}
