using System;
using ScriptDrawer.Core.Refs.Mappers;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core.Refs;

[Obsolete]
public class ImageUrlRef : UrlRef<Image, StreamToImageMapper>
{
    public ImageUrlRef(Uri url) : base(url) { }
}
