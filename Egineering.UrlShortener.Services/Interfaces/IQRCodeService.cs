using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Egineering.UrlShortener.Services.Interfaces
{
    public interface IQRCodeService
    {
        byte[] GenerateQRCode(string encodedUrl, int hue = 205, int saturation = 31, int lightness = 46);
    }
}
