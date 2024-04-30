using Contentful.Core;
using Contentful.Core.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContentfulApp.Services
{
    public class ContentfulService
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
    }
}

