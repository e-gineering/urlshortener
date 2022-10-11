namespace Egineering.UrlShortener.Services.Interfaces;

public interface IAzureTableStorageService
{
    Task<string> GetUrlFromVanityAsync(string vanity);
    IEnumerable<ShortenedUrl> GetAllPublicUrls();
    Task AddUrl(UrlRequest urlRequest);
    Task ReplaceUrl(UrlRequest urlRequest);
}
