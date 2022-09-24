using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ScriptDrawer.Core;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Demo;

internal class MainViewModel
{
    private readonly Subject<Config> configSubject = new();

    public MainViewModel(IOptionsMonitor<Config> configMonitor)
    {
        var publisher = new DelegatePublisher((name, image) => PublishedImages.Add(new PublishedImage(name, image)));

        configSubject
            .Select(config => config.Pipeline)
            .Select(pipelineFile => Observable.Create<string>(observer =>
            {
                observer.OnNext(pipelineFile);

                var fullPath = Path.GetFullPath(pipelineFile);
                var directoryName = Path.GetDirectoryName(pipelineFile)!;

                var fileSystemWatcher = new FileSystemWatcher(directoryName);
                fileSystemWatcher.EnableRaisingEvents = true;
                var subscription = Observable.FromEventPattern<FileSystemEventArgs>(fileSystemWatcher, nameof(fileSystemWatcher.Changed))
                    .Select(o => o.EventArgs.FullPath)
                    .Where(o => Path.GetFullPath(o) == fullPath)
                    .Subscribe(observer);

                return () =>
                {
                    subscription.Dispose();
                    fileSystemWatcher.Dispose();
                };
            }).Retry())
            .Switch()
            .Where(File.Exists)
            .SelectAsync(async pipelineFile =>
            {
                try
                {
                    var code = await File.ReadAllTextAsync(pipelineFile);
                    var scriptOptions = ScriptOptions.Default
                        .WithFilePath(Path.GetFullPath(pipelineFile))
                        .WithFileEncoding(Encoding.UTF8)
                        .WithReferences(typeof(IPipeline).Assembly)
                        .WithEmitDebugInformation(true);
                    var script = CSharpScript.Create<Type>(code, scriptOptions);
                    var pipelineType = (await script.RunAsync()).ReturnValue;
                    return (IPipeline) Activator.CreateInstance(pipelineType)!;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            })
            .WhereNotNull()
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
                    Console.WriteLine(e);
                }
            });

        configMonitor.OnChange(configSubject.OnNext);
        configSubject.OnNext(configMonitor.CurrentValue);
    }

    public ObservableCollection<PublishedImage> PublishedImages { get; } = new();
}

internal record PublishedImage(string Name, Image Image);
