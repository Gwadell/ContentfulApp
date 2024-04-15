using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Search;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {
        //private readonly IContentfulClient _client;

        public ExportController(IContentfulClient client)
        {
            //_client = client;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Export(ExportModel model)
        {
            //// Get the values from the space and access token provided by the user
            //var spaceId = model.SpaceId;
            //var accessToken = model.AccessToken;
            //var enviroment = model.EnvironmentName;
            //var contenttype = model.ContentTypesId;

            
            //var options = new ContentfulOptions
            //{
            //    SpaceId = spaceId,
            //    DeliveryApiKey = accessToken,
            //    Environment = enviroment,

            //};
            //var httpClient = new HttpClient();
            //var client = new ContentfulClient(httpClient, model.AccessToken, model.SpaceId, model.EnvironmentName);

            //var entries = await client.GetEntries();




            //// Get the values from the space and access token provided by the user
            //var spaceId = model.SpaceId;
            //var accessToken = model.AccessToken;
            //var environment = model.EnvironmentName;
            //var contentType = model.ContentTypesId;

            //// Configure Contentful client
            //var httpClient = new HttpClient();
            //var client = new ContentfulManagementClient(httpClient, accessToken, spaceId);

            //// Define query parameters based on model properties
            //var queryBuilder = new QueryBuilder<Entry<dynamic>>();

            //queryBuilder.ContentTypeIs(contentType);
            //queryBuilder.LocaleIs(model.Locales);
            

            //// Retrieve entries using the client and query builder
            //var entriesResponse = await client.GetEntriesCollection(queryBuilder);

            //// Filter entries based on environment
            //var filteredEntries = entriesResponse.Items.Where(entry => entry.SystemProperties.Environment.SystemProperties.Id == environment);
           

            //// You can now work with the 'filteredEntries' variable which contains the entries retrieved using the user-specific client

            return View();
        }
    }
}
