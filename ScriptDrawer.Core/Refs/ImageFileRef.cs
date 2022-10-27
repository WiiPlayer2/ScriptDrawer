using System;
using ScriptDrawer.Core.Refs.Mappers;
using SixLabors.ImageSharp;

namespace ScriptDrawer.Core.Refs;

[Obsolete]
public class ImageFileRef : FileRef<Image, StreamToImageMapper>
{
    public ImageFileRef(string filePath) : base(filePath) { }
}
