using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Errors;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;
using Newtonsoft.Json;
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

        public async Task<ContentfulManagementClient> GetManagementClient(string accessToken, string space, string environment)
        {
           var options = new ContentfulOptions
           {
                ManagementApiKey = accessToken,
                SpaceId = space,
                Environment = environment
            };

            return new ContentfulManagementClient(_httpClient, options);
        }

        public async Task<IEnumerable<object>> GetEntries(ContentfulClient client, string contentType, Type dtoType, string locale)
        {
            var skip = 0;
            const int batchSize = 10;

            var allEntries = new List<object>();
            try
            {
                do
                {   //vet inte vilken dtoType som ska användas, så använder reflection för att skapa en instans av QueryBuilder
                    var queryBuilderType = typeof(QueryBuilder<>).MakeGenericType(dtoType); // Create a generic type using reflection
                    var queryBuilder = Activator.CreateInstance(queryBuilderType); // Create an instance of generic type because the dtoType is not known at compile time, ony runtime

                    queryBuilderType.GetMethod("ContentTypeIs").Invoke(queryBuilder, new object[] { contentType });
                    queryBuilderType.GetMethod("LocaleIs").Invoke(queryBuilder, new object[] { locale });
                    queryBuilderType.GetMethod("Skip").Invoke(queryBuilder, new object[] { skip });
                    queryBuilderType.GetMethod("Include").Invoke(queryBuilder, new object[] { 0});
                    queryBuilderType.GetMethod("Limit").Invoke(queryBuilder, new object[] { batchSize });

                    var pageEntries = await client.GetEntries((dynamic)queryBuilder);

                    allEntries.AddRange(pageEntries);


                    skip += batchSize;
                } while (allEntries.Count % batchSize == 0);
            }
            catch (ContentfulException contEx)
            {
                throw new Exception("ContentfulException " + contEx.Message);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching entries: " + ex.Message);
            }
            return allEntries;
        }

        public async Task<IEnumerable<object>> GetEntriesForContentTypeAndLocale(ContentfulClient client, string contentType, string locale)
        {
            var dtoType = _dtoMappingService.GetDtoType(contentType);
            var entries = await GetEntries(client, contentType, dtoType, locale);

            var exportDtoType = _dtoMappingService.GetExportDtoType(contentType);
            return entries.Select(e => _dtoMappingService.MapToExportDto(e, exportDtoType));
        }

        public async Task<IEnumerable<Locale>> GetLocales(string accessToken, string environment, string spaceId)
        {
            var client = await GetClientAsync(accessToken, spaceId, environment);
            var localesCollection = await client.GetLocales();

            return localesCollection;
        }

        public async Task UpdateContent(string contentType, string locale, IEnumerable<object> data, string accessToken)
        {
            //// Create a management client with the provided access token
            //var managementClient = new ContentfulManagementClient(accessToken, ;

            //// For each object in the data collection
            //foreach (var item in data)
            //{
            //    // Convert the object to a dictionary
            //    var fields = item as IDictionary<string, object>;

            //    // Create a new Entry<dynamic> object
            //    var entry = new Entry<dynamic>();

            //    // Set the fields of the entry
            //    entry.Fields = new Dictionary<string, object>();

            //    // For each field in the dictionary
            //    foreach (var field in fields)
            //    {
            //        // Add the field to the entry
            //        entry.Fields.Add(field.Key, field.Value);
            //    }

            //    // Update the entry in Contentful
            //    await managementClient.UpdateEntry(entry, contentType, locale);
            //}
        }

    }

    public interface IContentfulService
    {
        Task<ContentfulClient> GetClientAsync(string accessToken, string spaceId, string environment);
        Task<ContentfulManagementClient> GetManagementClient(string accessToken, string space, string environment);
        Task<IEnumerable<object>> GetEntries(ContentfulClient client, string contentType, Type dtoType, string locale);
        Task<IEnumerable<object>> GetEntriesForContentTypeAndLocale(ContentfulClient client, string contentType, string locale);

        Task<IEnumerable<Locale>> GetLocales(string accessToken, string environment, string spaceId);

        Task UpdateContent(string contentType, string locale, IEnumerable<object> data, string accessToken);

    }
}

