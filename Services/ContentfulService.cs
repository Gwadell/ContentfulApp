using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Search;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContentfulApp.Services
{
    public class ContentfulService : IContentfulService
    {
        private readonly HttpClient _httpClient;

        public ContentfulService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ContentfulClient> GetClientAsync(string accessToken, string spaceId, string environment)
        {
            var options = new ContentfulOptions
            {
                DeliveryApiKey = accessToken,
                SpaceId = spaceId,
                Environment = environment
            };

            return new ContentfulClient(_httpClient, options);
        }

        public async Task<IEnumerable<object>> GetEntriesForContentType(ContentfulClient client, string contentType, Type dtoType, string locale)
        {
            var skip = 0;
            const int batchSize = 10;

            var allEntries = new List<object>();

            do
            {
                var queryBuilderType = typeof(QueryBuilder<>).MakeGenericType(dtoType); // Create a generic type using reflection
                var queryBuilder = Activator.CreateInstance(queryBuilderType); // Create an instance of generic type because i dtoType is not known at compile time, ony runtime

                queryBuilderType.GetMethod("ContentTypeIs").Invoke(queryBuilder, new object[] { contentType });
                queryBuilderType.GetMethod("LocaleIs").Invoke(queryBuilder, new object[] { locale });
                queryBuilderType.GetMethod("Skip").Invoke(queryBuilder, new object[] { skip });
                queryBuilderType.GetMethod("Include").Invoke(queryBuilder, new object[] { 1 });
                queryBuilderType.GetMethod("Limit").Invoke(queryBuilder, new object[] { batchSize });

                var pageEntries = await client.GetEntries((dynamic)queryBuilder);

                allEntries.AddRange(pageEntries);


                skip += batchSize;
            } while (allEntries.Count % batchSize == 0);

            return allEntries;
        }
    }

    public interface IContentfulService
    {
        Task<ContentfulClient> GetClientAsync(string accessToken, string spaceId, string environment);
        Task<IEnumerable<object>> GetEntriesForContentType(ContentfulClient client, string contentType, Type dtoType, string locale);
    }
}

