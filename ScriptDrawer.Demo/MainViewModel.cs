using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using DynamicData.Kernel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ScriptDrawer.Core;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using Path = System.IO.Path;

namespace ScriptDrawer.Demo;

internal class MainViewModel
{
    private readonly Subject<Config> configSubject = new();

    public MainViewModel(IOptionsMonitor<Config> configMonitor, ILogger<MainViewModel> logger)
    {
        var publisher = new DelegatePublisher((name, image) => PublishedImages.Add(new PublishedImage(name, image)));

        configSubject.Select(config => config.Pipeline)
            .CompilePipeline(logger)
            .ObserveOnDispatcher()
            .SelectAsync(async (pipeline, cancellationToken) =>
            {
                await ExecutePipelineAsync(pipeline, logger, publisher, cancellationToken);
                return Unit.Default;
            })
            .Subscribe();

        configMonitor.OnChange(configSubject.OnNext);
        configSubject.OnNext(configMonitor.CurrentValue);
    }

    public ObservableCollection<PublishedImage> PublishedImages { get; } = new();

    private async Task ExecutePipelineAsync(IPipeline pipeline, ILogger logger, IPublisher publisher, CancellationToken cancellationToken)
    {
        PublishedImages.Clear();
        try
        {
            await pipeline.ExecuteAsync(publisher, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Pipeline failed.");
        }
    }
}

internal static class RenderExtensions
{
    private static IObservable<IPipeline> Compile(this IObservable<string> code, ILogger logger)
        => code.SelectAsync(async (code, cancellationToken) =>
            {
                try
                {
                    var scriptOptions = ScriptOptions.Default
                        .WithReferences(
                            typeof(IPipeline).Assembly,
                            typeof(Image).Assembly,
                            typeof(RectangularPolygon).Assembly);
                    var script = CSharpScript.Create<Type>(code, scriptOptions);
                    var pipelineType = (await script.RunAsync(cancellationToken: cancellationToken)).ReturnValue;
                    return (IPipeline) Activator.CreateInstance(pipelineType)!;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to compile pipeline.");
                    return null;
                }
            })
            .WhereNotNull();

    public static IObservable<IPipeline> CompilePipeline(this IObservable<string> pipelineFile, ILogger logger)
        => pipelineFile
            .GetPipelineCode()
            .Compile(logger);


    private static IObservable<string> GetPipelineCode(this IObservable<string> pipelineFile)
    {
        return pipelineFile
            .Select(file => WatchFile(file)
                .SelectAsync((_, cancellationToken) => File.ReadAllTextAsync(file, cancellationToken))
                .RetryWithBackOff((Exception _, int count) => TimeSpan.FromSeconds(Math.Min(count, 15))))
            .Switch();

        IObservable<Unit> WatchFile(string file)
            => Observable.Create<Unit>(observer =>
            {
                observer.OnNext(Unit.Default);

                var fullPath = Path.GetFullPath(file);
                var directoryName = Path.GetDirectoryName(file)!;

                var fileSystemWatcher = new FileSystemWatcher(directoryName);
                fileSystemWatcher.EnableRaisingEvents = true;
                var subscription = Observable.FromEventPattern<FileSystemEventArgs>(fileSystemWatcher, nameof(fileSystemWatcher.Changed))
                    .Select(o => o.EventArgs.FullPath)
                    .Where(o => Path.GetFullPath(o) == fullPath && File.Exists(fullPath))
                    .Select(_ => Unit.Default)
                    .Subscribe(observer);

                return () =>
                {
                    subscription.Dispose();
                    fileSystemWatcher.Dispose();
                };
            });
    }
}

internal record PublishedImage(string Name, Image Image);
