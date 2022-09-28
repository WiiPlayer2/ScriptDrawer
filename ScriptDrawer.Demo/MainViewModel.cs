using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using DynamicData.Kernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ScriptDrawer.Core;
using ScriptDrawer.Serialization;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Demo;

internal class MainViewModel
{
    private readonly Subject<Config> configSubject = new();

    public MainViewModel(
        IOptionsMonitor<Config> configMonitor,
        ILogger<MainViewModel> logger,
        ConfigSerializer configSerializer,
        Engine engine)
    {
        var publisher = new DelegatePublisher((name, image) => PublishedImages.Add(new PublishedImage(name, image)));

        var pipelines = configSubject
            .Select(config => config.Pipeline)
            .CompilePipeline(logger, engine);

        var configurationContent = configSubject
            .Select(config => config.Configuration)
            .ReadConfigurationContent(logger, configSerializer);

        pipelines.CombineLatest(configurationContent, (pipeline, configurationContent) =>
            {
                var configuration = configSerializer.Deserialize(configurationContent, pipeline.ConfigurationType);
                return (pipeline, configuration);
            })
            .RetryWithDefaultBackOff()
            .ObserveOnDispatcher()
            .SelectAsync(async (pair, cancellationToken) =>
            {
                var (pipeline, configuration) = pair;
                await ExecutePipelineAsync(engine, pipeline, logger, publisher, configuration, cancellationToken);
                return Unit.Default;
            })
            .Subscribe();

        configMonitor.OnChange(configSubject.OnNext);
        configSubject.OnNext(configMonitor.CurrentValue);
    }

    public ObservableCollection<PublishedImage> PublishedImages { get; } = new();

    private async Task ExecutePipelineAsync(Engine engine, IPipeline pipeline, ILogger logger, IPublisher publisher, PipelineConfig? configuration, CancellationToken cancellationToken)
    {
        PublishedImages.Clear();
        try
        {
            await engine.RunPipelineAsync(pipeline, configuration, publisher, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Pipeline failed.");
        }
    }
}

internal static class RenderExtensions
{
    private static IObservable<IPipeline> Compile(this IObservable<string> code, ILogger logger, Engine engine)
        => code.SelectAsync(async (code, cancellationToken) =>
            {
                try
                {
                    return await engine.CompilePipelineAsync(code, cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to compile pipeline.");
                    return null;
                }
            })
            .WhereNotNull();

    public static IObservable<IPipeline> CompilePipeline(this IObservable<string> pipelineFile, ILogger logger, Engine engine)
        => pipelineFile
            .GetPipelineCode()
            .Compile(logger, engine);


    private static IObservable<string> GetPipelineCode(this IObservable<string> pipelineFile)
    {
        return pipelineFile
            .Select(file => WatchFile(file)
                .SelectAsync((_, cancellationToken) => File.ReadAllTextAsync(file, cancellationToken))
                .RetryWithDefaultBackOff())
            .Switch()
            .DistinctUntilChanged();
    }

    public static IObservable<string> ReadConfigurationContent(this IObservable<string> configurationFile, ILogger logger, ConfigSerializer serializer)
        => configurationFile
            .Select(file => WatchFile(file)
                .SelectAsync((_, cancellationToken) => File.ReadAllTextAsync(file, cancellationToken))
                .RetryWithDefaultBackOff())
            .Switch()
            .DistinctUntilChanged();

    public static IObservable<T> RetryWithDefaultBackOff<T>(this IObservable<T> observable)
        => observable.RetryWithBackOff((Exception _, int count) => TimeSpan.FromSeconds(Math.Min(count, 15)));

    private static IObservable<Unit> WatchFile(string file)
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

internal record PublishedImage(string Name, Image Image);
