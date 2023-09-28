namespace Egineering.UrlShortener.Services.Interfaces;

public interface IQRCodeService
{
    byte[] GenerateQRCode(string url);
}
