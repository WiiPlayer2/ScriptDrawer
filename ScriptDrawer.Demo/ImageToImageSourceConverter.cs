using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ScriptDrawer.Demo;

[ValueConversion(typeof(Image), typeof(ImageSource))]
internal class ImageToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Image image)
            throw new ArgumentException(nameof(value));

        using var img = image.CloneAs<Bgra32>();
        var buffer = new byte[img.Width * img.Height * 4];
        img.CopyPixelDataTo(buffer);
        var pixelFormat = Map(img);
        var bmp = new WriteableBitmap(image.Width, image.Height, image.Metadata.HorizontalResolution, image.Metadata.VerticalResolution, pixelFormat, default);
        bmp.Lock();
        Marshal.Copy(buffer, 0, bmp.BackBuffer, buffer.Length);
        bmp.AddDirtyRect(new Int32Rect(0, 0, img.Width, img.Height));
        bmp.Unlock();
        return bmp;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

    private PixelFormat Map(Image image)
    {
        var imageType = image.GetType();
        var pixelType = imageType.GenericTypeArguments[0];
        if (pixelType == typeof(Bgra32))
            return PixelFormats.Bgra32;
        throw new NotImplementedException();
    }
}
