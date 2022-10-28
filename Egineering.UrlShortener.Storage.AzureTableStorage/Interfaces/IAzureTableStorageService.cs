using Egineering.UrlShortener.Storage.DTOs;
using Egineering.UrlShortener.Storage.Interfaces;

namespace Egineering.UrlShortener.Storage.AzureTableStorage.Interfaces;

public interface IAzureTableStorageService : IStorageService
{
    Task<string> GetUrlFromVanityAsync(string vanity);
    //public string PartitionKey { get; set; }
    IEnumerable<ShortenedUrl> GetAllPublicUrls();
    Task AddUrl(UrlRequest urlRequest);
    Task ReplaceUrl(UrlRequest urlRequest);
}
