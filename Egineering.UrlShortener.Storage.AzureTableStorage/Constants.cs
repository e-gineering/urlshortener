namespace Egineering.UrlShortener.Storage.AzureTableStorage
{
    internal static class Constants
    {
        // Azure Storage
        public const string AzureStorageConnectionString = "AzureStorage";
        public const string UrlTableName = "urls";
        
        // URL properties
        public const string Name = "name";
        public const string Url = "url";
        public const string UrlPartitionKey = Url;
        public const string Visits = "visits";
        public const string IsPublic = "isPublic";
        
        // Azure Request Error Codes
        public static class AzureRequestErrorCodes
        {
            public const string ResourceNotFound = "ResourceNotFound";
        }
    }
}
