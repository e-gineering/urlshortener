namespace Egineering.UrlShortener.Services;

public class AzureTableStorageService : IAzureTableStorageService
{
    private readonly TableClient _tableClient;
    private readonly TableServiceClient _tableServiceClient;

    const string Url = "url";
    const string UrlPartitionKey = Url;

    const string Visits = "visits";

    public AzureTableStorageService(IConfiguration configuration)
    {
        var azureStorageConnectionString = configuration.GetConnectionString(Constants.AzureStorageConnectionString);

        _tableServiceClient = new TableServiceClient(azureStorageConnectionString);
        _tableClient = GetUrlTableClient();
    }

    public async Task<string> GetUrlFromVanityAsync(string vanity)
    {
        var urlEntity = await GetUrlEntityByVanity(vanity);

        var urlEntityUrl = urlEntity.GetString(Url);
        var currentUrlVisits = urlEntity.GetInt32(Visits);

        var entity = new TableEntity(UrlPartitionKey, vanity)
        {
            { Url, urlEntityUrl },
            { Visits, currentUrlVisits + 1 }
        };

        await _tableClient.UpdateEntityAsync(entity, ETag.All);

        return urlEntityUrl;
    }

    public IEnumerable<ShortenedUrl> GetAllShortenedUrls()
    {
        var tableEntities = _tableClient.Query<TableEntity>(entity => entity.PartitionKey == Url);

        var results = tableEntities.Select(entity => new ShortenedUrl
        {
            PartitionKey = entity.PartitionKey,
            Timestamp = entity.Timestamp.Value,
            Url = entity.GetString(Url),
            Vanity = entity.RowKey,
            Visits = entity.GetInt32(Visits).Value
        });

        return results;
    }

    public async Task SaveUrl(UrlRequest urlRequest)
    {
        try
        {
            var urlEntity = await GetUrlEntityByVanity(urlRequest.Vanity);

            var entity = new TableEntity(UrlPartitionKey, urlRequest.Vanity)
            {
                { Url, urlRequest.Url },
                { Visits, urlEntity.GetInt32(Visits) }
            };

            await _tableClient.UpdateEntityAsync(entity, ETag.All);
        }
        catch (RequestFailedException requestFailedException)
        {
            if (requestFailedException.Status != 404) throw requestFailedException;

            var entity = new TableEntity(UrlPartitionKey, urlRequest.Vanity)
            {
                { Url, urlRequest.Url },
                { Visits, 0 }
            };

            await _tableClient.AddEntityAsync(entity);
        }
    }

    private TableClient GetUrlTableClient()
    {
        _tableServiceClient.CreateTableIfNotExists(Constants.UrlTableName);

        return _tableServiceClient.GetTableClient(Constants.UrlTableName);
    }

    private async Task<TableEntity> GetUrlEntityByVanity(string vanity)
    {
        return await _tableClient.GetEntityAsync<TableEntity>(UrlPartitionKey, vanity);
    }
}
