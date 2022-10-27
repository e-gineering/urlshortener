
namespace Egineering.UrlShortener.Storage.Interfaces
{
    public interface IStorageBuilder
    {
        Dictionary<string, Type> StorageOptions { get; }
        Type? FindStorageOption(string name);
    }
}