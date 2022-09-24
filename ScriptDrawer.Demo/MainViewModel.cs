using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
            .Where(File.Exists)
            .SelectAsync(async pipelineFile =>
            {
                try
                {
                    await using var fileStream = File.OpenRead(pipelineFile);
                    var scriptOptions = ScriptOptions.Default
                        .WithFilePath(Path.GetFullPath(pipelineFile))
                        .WithReferences(typeof(IPipeline).Assembly)
                        .WithEmitDebugInformation(true);
                    var script = CSharpScript.Create<Type>(fileStream, scriptOptions);
                    var pipelineType = (await script.RunAsync()).ReturnValue;
                    return (IPipeline) Activator.CreateInstance(pipelineType)!;
                }
                catch (Exception e)
                {
                    return null;
                }
            })
            .WhereNotNull()
            .Subscribe(async pipeline =>
            {
                PublishedImages.Clear();
                await pipeline.ExecuteAsync(publisher);
            });

        configMonitor.OnChange(configSubject.OnNext);
        configSubject.OnNext(configMonitor.CurrentValue);
    }

    public ObservableCollection<PublishedImage> PublishedImages { get; } = new();
}

internal record PublishedImage(string Name, Image Image);
