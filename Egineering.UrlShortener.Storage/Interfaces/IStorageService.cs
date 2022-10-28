
using Egineering.UrlShortener.Storage.DTOs;

namespace Egineering.UrlShortener.Storage.Interfaces
{
    public interface IStorageService
    {
        bool IsStorageType(string name);
        string StorageType();

        Task<string> GetUrlFromVanityAsync(string vanity);
        IEnumerable<ShortenedUrl> GetAllPublicUrls();
        Task AddUrl(UrlRequest urlRequest);
        Task ReplaceUrl(UrlRequest urlRequest);
    }
}
