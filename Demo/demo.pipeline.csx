#r "../ScriptDrawer.Shared.dll"
#r "../SixLabors.ImageSharp.dll"
#r "../SixLabors.ImageSharp.Drawing.dll"
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
    public IRef<Image> AvatarRef { get; init; }

    public IRef<Image> FlagRef { get; init; }

    public float Radius { get; init; } // 0..1

    public int CirclePoints { get; init; }
}

public class Demo : IPipeline<Config>
{
    async Task IPipeline<Config>.ExecuteAsync(IPublisher publisher, Config configuration, CancellationToken cancellationToken)
    {
        var avatarImage = await configuration.AvatarRef.ResolveAsync(cancellationToken);
        if(avatarImage.Size().Width != avatarImage.Size().Height)
            throw new InvalidOperationException("avatar not square");
        await publisher.PublishAsync("avatar", avatarImage, cancellationToken);

        var flagImage = await configuration.FlagRef.ResolveAsync(cancellationToken);
        await publisher.PublishAsync("flag", flagImage, cancellationToken);

        var ratio = flagImage.Size().Width / (double)flagImage.Size().Height;
        var width = (int)(ratio * avatarImage.Height);
        var squareFlag = flagImage.Clone(x => x
            .Resize(width, avatarImage.Height)
            .Crop(avatarImage.Width, avatarImage.Height));
        await publisher.PublishAsync("squareFlag", squareFlag, cancellationToken);

        var fullRadius = squareFlag.Size().Height / 2f;
        var radius = fullRadius * configuration.Radius;
        var mask = squareFlag.Clone(x => x
            .Fill(
                new DrawingOptions()
                {
                    GraphicsOptions = {
                        AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
                    },
                },
                Color.White,
                new RegularPolygon((PointF)(Point)(squareFlag.Size() / 2), configuration.CirclePoints, radius)));
        await publisher.PublishAsync("mask", mask, cancellationToken);

        var result = avatarImage.Clone(x => x
            .DrawImage(mask, 1f));
        await publisher.PublishAsync("result", result, cancellationToken);
    }
}

return typeof(Demo);
