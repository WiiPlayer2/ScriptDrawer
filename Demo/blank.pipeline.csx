#r "nuget: ScriptDrawer.Shared, 0.1.0-1664441847"
#r "nuget: SixLabors.ImageSharp, 2.1.3"
#r "nuget: SixLabors.ImageSharp.Drawing, 1.0.0-beta15"
#nullable enable

using System;
using System.Threading.Tasks;
using ScriptDrawer.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Threading;

public class Config
{

}

public class Blank : IPipeline<Config>
{
    async Task IPipeline<Config>.ExecuteAsync(IPublisher publisher, Config configuration, CancellationToken cancellationToken)
    {
        using var image = new Image<Bgra32>(800, 600);
        image.Mutate(x => x.Fill(Color.Green));
        await publisher.PublishAsync("result", image, cancellationToken);
    }
}

return typeof(Blank);
