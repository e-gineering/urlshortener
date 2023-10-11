using Egineering.UrlShortener.Services.Exceptions;

namespace Egineering.UrlShortener.Services;

public class AzureTableStorageService : IAzureTableStorageService
{
    private readonly TableClient _tableClient;
    private readonly TableServiceClient _tableServiceClient;

    public AzureTableStorageService(IConfiguration configuration)
    {
        var azureStorageConnectionString = configuration.GetConnectionString(Constants.AzureStorageConnectionString);

        _tableServiceClient = new TableServiceClient(azureStorageConnectionString);
        _tableClient = GetUrlTableClient();
    }

    public async Task<string> GetUrlFromVanityAsync(string vanity)
    {
        var urlEntity = await GetUrlEntityByVanity(vanity);

        if (urlEntity == null)
        {
            throw new UrlEntityNotFoundException(vanity);
        }

        var urlEntityName = urlEntity.GetString(Constants.Name);
        var urlEntityUrl = urlEntity.GetString(Constants.Url);
        var currentUrlVisits = urlEntity.GetInt32(Constants.Visits);
        var isPublic = urlEntity.GetBoolean(Constants.IsPublic);

        var entity = new TableEntity(Constants.UrlPartitionKey, vanity)
        {
            { Constants.Name, urlEntityName },
            { Constants.Url, urlEntityUrl },
            { Constants.Visits, currentUrlVisits + 1 },
            { Constants.IsPublic, isPublic }
        };

        await _tableClient.UpdateEntityAsync(entity, ETag.All);

        return urlEntityUrl;
    }

    public IEnumerable<ShortenedUrl> GetAllPublicUrls()
    {
        var tableEntities = _tableClient.Query<TableEntity>(entity => entity.PartitionKey == Constants.Url && entity.GetBoolean(Constants.IsPublic).Value == true);

        var results = tableEntities.Select(entity => new ShortenedUrl
        {
            Name = entity.GetString(Constants.Name),
            PartitionKey = entity.PartitionKey,
            Timestamp = entity.Timestamp.Value,
            Url = entity.GetString(Constants.Url),
            Vanity = entity.RowKey,
            Visits = entity.GetInt32(Constants.Visits).Value,
            IsPublic = entity.GetBoolean(Constants.IsPublic).Value
        })
        .OrderBy(url => url.Name);

        return results;
    }

    public async Task<string> AddUrl(UrlRequest urlRequest)
    {
        if (string.IsNullOrWhiteSpace(urlRequest.Vanity))
        {
            // User did not provide a vanity; generate a random one
            urlRequest.Vanity = await GenerateVanityAsync();
            // Randomly generated urls are not publicly displayed
            urlRequest.IsPublic = false;
        }
        else
        {
            var urlEntity = await GetUrlEntityByVanity(urlRequest.Vanity);

            if (urlEntity != null)
            {
                throw new ConflictException(urlRequest.Vanity);
            }
        }

        var entity = new TableEntity(Constants.UrlPartitionKey, urlRequest.Vanity)
        {
            { Constants.Name, urlRequest.Name },
            { Constants.Url, urlRequest.Url },
            { Constants.Visits, 0 },
            { Constants.IsPublic, urlRequest.IsPublic }
        };

        await _tableClient.AddEntityAsync(entity);

        return urlRequest.Vanity;
    }

    private async Task<string> GenerateVanityAsync()
    {
        var rng = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string vanity = string.Empty;
        TableEntity? urlEntity = null;
        do
        {
            vanity = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[rng.Next(s.Length)]).ToArray());

            urlEntity = await GetUrlEntityByVanity(vanity);
        }
        while (urlEntity != null);

        return vanity;
    }

    public async Task ReplaceUrl(UrlRequest urlRequest)
    {
        var urlEntity = await GetUrlEntityByVanity(urlRequest.Vanity);

        if (urlEntity == null)
        {
            throw new UrlEntityNotFoundException(urlRequest.Vanity);
        }

        var entity = new TableEntity(Constants.UrlPartitionKey, urlRequest.Vanity)
            {
                { Constants.Name, urlRequest.Name },
                { Constants.Url, urlRequest.Url },
                { Constants.Visits, urlEntity.GetInt32(Constants.Visits) },
                { Constants.IsPublic, urlRequest.IsPublic },
            };

        await _tableClient.UpdateEntityAsync(entity, ETag.All);
    }

    private TableClient GetUrlTableClient()
    {
        _tableServiceClient.CreateTableIfNotExists(Constants.UrlTableName);

        return _tableServiceClient.GetTableClient(Constants.UrlTableName);
    }

    private async Task<TableEntity?> GetUrlEntityByVanity(string vanity)
    {
        TableEntity? tableEntity = null;
        try
        {
            tableEntity = await _tableClient.GetEntityAsync<TableEntity>(Constants.UrlPartitionKey, vanity);
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == Constants.AzureRequestErrorCodes.ResourceNotFound)
                return null;
        }
        return tableEntity;
    }
}
