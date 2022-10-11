namespace Egineering.UrlShortener.Services.DTOs;

[ExcludeFromCodeCoverage]
public class ShortenedUrl
{
    public string Name { get; set; }
    public string PartitionKey { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Url { get; set; }
    public string Vanity { get; set; }
    public int Visits { get; set; }
    public bool IsPublic { get; set; }
}
