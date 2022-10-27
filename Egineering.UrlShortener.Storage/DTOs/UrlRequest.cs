using System.Diagnostics.CodeAnalysis;

namespace Egineering.UrlShortener.Storage.DTOs;

[ExcludeFromCodeCoverage]
public class UrlRequest
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Vanity { get; set; }
    public bool IsPublic { get; set; }
}
