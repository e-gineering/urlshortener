namespace Egineering.UrlShortener.Services.Interfaces;

public interface IAzureTableStorageService
{
    Task<string> GetUrlFromVanityAsync(string vanity);
    IEnumerable<ShortenedUrl> GetAllShortenedUrls();
    Task SaveUrl(UrlRequest urlRequest);
}
