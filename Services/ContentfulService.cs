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
        private readonly IDtoMappingService _dtoMappingService;
        private readonly IContentfulService _contentfulService;

        public ContentfulService(HttpClient httpClient, IDtoMappingService dtoMappingService)
        {
            _httpClient = httpClient;
            _dtoMappingService = dtoMappingService;
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
            const int batchSize = 50;

            var allEntries = new List<object>();

            do
            {   //vet inte vilken dtoType som ska användas, så vi använder reflection för att skapa en instans av QueryBuilder
                var queryBuilderType = typeof(QueryBuilder<>).MakeGenericType(dtoType); // Create a generic type using reflection
                var queryBuilder = Activator.CreateInstance(queryBuilderType); // Create an instance of generic type because the dtoType is not known at compile time, ony runtime

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

        public async Task<IEnumerable<object>> GetEntriesForContentTypeAndLocale(ContentfulClient client, string contentType, string locale)
        {
            var dtoType = _dtoMappingService.GetDtoType(contentType);
            var entries = await GetEntriesForContentType(client, contentType, dtoType, locale);

            var exportDtoType = _dtoMappingService.GetExportDtoType(contentType);
            return entries.Select(e => _dtoMappingService.MapToExportDto(e, exportDtoType));
        }
    }

    public interface IContentfulService
    {
        Task<ContentfulClient> GetClientAsync(string accessToken, string spaceId, string environment);
        Task<IEnumerable<object>> GetEntriesForContentType(ContentfulClient client, string contentType, Type dtoType, string locale);
        Task<IEnumerable<object>> GetEntriesForContentTypeAndLocale(ContentfulClient client, string contentType, string locale);
    }
}

