using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Contentful.Core.Search;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using ContentfulApp.Models.DTO;
using System.Dynamic;
using Microsoft.AspNetCore.Http.Features;

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
            int skip = 0;
            const int batchSize = 80;

            var httpClient = new HttpClient();
            var options = new ContentfulOptions
            {
                DeliveryApiKey = model.AccessToken,
                SpaceId = model.SpaceId,
                Environment = model.Environment
            };
            var client = new ContentfulClient(httpClient, options);

            List<EntryPlp> allEntries = new List<EntryPlp>();

            do
            {
                var queryBuilder = new QueryBuilder<EntryPlp>();

                queryBuilder.ContentTypeIs(model.ContentType);
                queryBuilder.LocaleIs(model.Locale);
                queryBuilder.Skip(skip);
                queryBuilder.Include(2);
                queryBuilder.Limit(batchSize);

                var pageEntries = await client.GetEntries(queryBuilder);


                dynamic response = JsonConvert.SerializeObject(pageEntries);

                allEntries.AddRange(pageEntries);

                skip += batchSize;
            } while (allEntries.Count % batchSize == 0);

            return View(allEntries);
        }
    }
}
