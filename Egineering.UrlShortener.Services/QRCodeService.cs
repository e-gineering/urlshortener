using SkiaSharp;
using SkiaSharp.QrCode;
using SkiaSharp.QrCode.Models;

namespace Egineering.UrlShortener.Services;

public class QRCodeService : IQRCodeService
{
    public byte[] GenerateQRCode(string encodedUrl, int hue = 205, int saturation = 31, int lightness = 46)
    {
        // url decode the encodedUrl
        var url = Uri.UnescapeDataString(encodedUrl);

        using var generator = new QRCodeGenerator();
        var qr = generator.CreateQrCode(url, ECCLevel.L, quietZoneSize: 1);

        var info = new SKImageInfo(512, 512);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        var logo = File.ReadAllBytes("wwwroot/images/EG.png");
        var icon = new IconData
        {
            Icon = SKBitmap.Decode(logo),
            IconSizePercent = 15,
        };

        canvas.Render(qr, info.Width, info.Height, SKColor.Empty, SKColor.FromHsl(hue, saturation, lightness), icon);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}

