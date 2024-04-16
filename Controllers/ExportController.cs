using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Search;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Export(ExportModel model)
        {
            var httpClient = new HttpClient();

            var options = new ContentfulOptions
            {
                DeliveryApiKey = model.AccessToken,
                SpaceId = model.SpaceId,
                Environment = model.EnvironmentName
            };

            var client = new ContentfulClient(httpClient, options);

            // Define variables for pagination
            int skip = 0;
            const int batchSize = 80;

            List<Entry<dynamic>> allEntries = new List<Entry<dynamic>>();
            do
            {
                var queryBuilder = new QueryBuilder<Entry<dynamic>>();

                queryBuilder.ContentTypeIs(model.ContentTypesId);
                queryBuilder.LocaleIs(model.Locales);
                queryBuilder.Skip(skip);
                queryBuilder.Include(1);
                queryBuilder.Limit(batchSize);
                
                var pageEntries = await client.GetEntries(queryBuilder);

                allEntries.AddRange(pageEntries);

                skip += batchSize;
            } while (allEntries.Count % batchSize == 0);

            return View();
        }
    }
}
