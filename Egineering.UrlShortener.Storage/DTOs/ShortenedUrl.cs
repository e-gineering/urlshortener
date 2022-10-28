using System.Diagnostics.CodeAnalysis;

namespace Egineering.UrlShortener.Storage.DTOs;

[ExcludeFromCodeCoverage]
public class ShortenedUrl
{
    public string Name { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Url { get; set; }
    public string Vanity { get; set; }
    public int Visits { get; set; }
    public bool IsPublic { get; set; }
}
