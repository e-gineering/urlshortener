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

        var urlEntityName = urlEntity.GetString(Constants.Name);
        var urlEntityUrl = urlEntity.GetString(Constants.Url);
        var currentUrlVisits = urlEntity.GetInt32(Constants.Visits);

        var entity = new TableEntity(Constants.UrlPartitionKey, vanity)
        {
            { Constants.Name, urlEntityName },
            { Constants.Url, urlEntityUrl },
            { Constants.Visits, currentUrlVisits + 1 }
        };

        await _tableClient.UpdateEntityAsync(entity, ETag.All);

        return urlEntityUrl;
    }

    public IEnumerable<ShortenedUrl> GetAllShortenedUrls()
    {
        var tableEntities = _tableClient.Query<TableEntity>(entity => entity.PartitionKey == Constants.Url);

        var results = tableEntities.Select(entity => new ShortenedUrl
        {
            Name = entity.GetString(Constants.Name),
            PartitionKey = entity.PartitionKey,
            Timestamp = entity.Timestamp.Value,
            Url = entity.GetString(Constants.Url),
            Vanity = entity.RowKey,
            Visits = entity.GetInt32(Constants.Visits).Value
        })
        .OrderBy(url => url.Name);

        return results;
    }

    public async Task AddUrl(UrlRequest urlRequest)
    {
        var urlEntity = await GetUrlEntityByVanity(urlRequest.Vanity);

        if (urlEntity != null)
        {
            throw new ConflictException(urlRequest.Vanity);
        }

        var entity = new TableEntity(Constants.UrlPartitionKey, urlRequest.Vanity)
        {
            { Constants.Name, urlRequest.Name },
            { Constants.Url, urlRequest.Url },
            { Constants.Visits, 0 }
        };

        await _tableClient.AddEntityAsync(entity);
    }

    public async Task ReplaceUrl(UrlRequest urlRequest)
    {
        try
        {
            var urlEntity = await GetUrlEntityByVanity(urlRequest.Vanity);

            var entity = new TableEntity(Constants.UrlPartitionKey, urlRequest.Vanity)
            {
                { Constants.Name, urlRequest.Name },
                { Constants.Url, urlRequest.Url },
                { Constants.Visits, urlEntity.GetInt32(Constants.Visits) }
            };

            await _tableClient.UpdateEntityAsync(entity, ETag.All);
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == "ResourceNotFound")
                throw new UrlEntityNotFoundException(urlRequest.Vanity);

            throw ex;
        }
    }

    private TableClient GetUrlTableClient()
    {
        _tableServiceClient.CreateTableIfNotExists(Constants.UrlTableName);

        return _tableServiceClient.GetTableClient(Constants.UrlTableName);
    }

    private async Task<TableEntity> GetUrlEntityByVanity(string vanity)
    {
        return await _tableClient.GetEntityAsync<TableEntity>(Constants.UrlPartitionKey, vanity);
    }
}
