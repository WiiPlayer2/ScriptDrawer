using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ScriptDrawer.Core;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Demo;

internal class MainViewModel
{
    private readonly Subject<Config> configSubject = new();

    public MainViewModel(IOptionsMonitor<Config> configMonitor, ILogger<MainViewModel> logger)
    {
        var publisher = new DelegatePublisher((name, image) => PublishedImages.Add(new PublishedImage(name, image)));

        GetPipelineCode(configSubject.Select(config => config.Pipeline))
            .Compile(logger)
            .ObserveOnDispatcher()
            .Subscribe(async pipeline =>
            {
                PublishedImages.Clear();
                try
                {
                    await pipeline.ExecuteAsync(publisher);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Pipeline failed.");
                }
            });

        configMonitor.OnChange(configSubject.OnNext);
        configSubject.OnNext(configMonitor.CurrentValue);
    }

    public ObservableCollection<PublishedImage> PublishedImages { get; } = new();

    private IObservable<string> GetPipelineCode(IObservable<string> pipelineFile)
        => pipelineFile
            .Select(file => WatchFile(file).SelectAsync(_ => File.ReadAllTextAsync(file)))
            .Switch();

    private IObservable<Unit> WatchFile(string file)
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

internal static class RenderExtensions
{
    public static IObservable<IPipeline> Compile(this IObservable<string> code, ILogger logger)
        => code.SelectAsync(async code =>
            {
                try
                {
                    var scriptOptions = ScriptOptions.Default
                        .WithReferences(typeof(IPipeline).Assembly);
                    var script = CSharpScript.Create<Type>(code, scriptOptions);
                    var pipelineType = (await script.RunAsync()).ReturnValue;
                    return (IPipeline) Activator.CreateInstance(pipelineType)!;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to compile pipeline.");
                    return null;
                }
            })
            .WhereNotNull();
}

internal record PublishedImage(string Name, Image Image);
