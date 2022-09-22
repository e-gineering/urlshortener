namespace Egineering.UrlShortener.Services.DTOs;

[ExcludeFromCodeCoverage]
public class UrlRequest
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Vanity { get; set; }
}
