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
using OfficeOpenXml;
using System.Reflection.Metadata.Ecma335;
using System.Linq.Expressions;
using System.Linq;
using Contentful.Core.Extensions;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(ExportModel model)
        {
            var contentTypes = model.ContentTypes.Split(',').Select(c => c.Trim()).ToList();


            //int skip = 0;
            //const int batchSize = 80;

            var httpClient = new HttpClient();
            var options = new ContentfulOptions
            {
                DeliveryApiKey = model.AccessToken,
                SpaceId = model.SpaceId,
                Environment = model.Environment
            };
            var client = new ContentfulClient(httpClient, options);

            //List<EntryPlpDto> allEntries = new List<EntryPlpDto>();

            var allEntries = new Dictionary<string, IEnumerable<object>>();

            foreach (var contentType in contentTypes)
            {
                var dtoType = contentType switch
                {
                    "productListingPage" => typeof(EntryPlpDto),
                    "brand" => typeof(EntryBrandDto),
                    _ => throw new ArgumentException($"the contenttype:{contentType} does not exist")
                }; 

                var entries = await GetEntriesForContentType(client, model, contentType, dtoType);
                allEntries.Add(contentType, entries);
            }

            var environmentName = model.Environment == "master" ? "" : model.Environment;
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm"); 

            var excelFileName = $"export-{environmentName}-{currentDateTime}.xlsx";
            var excelfilePath = Path.Combine(Path.GetTempPath(), excelFileName);

            ExportToExcel(allEntries, excelfilePath);

            return Content(excelfilePath);
        }

        private async Task<IEnumerable<object>> GetEntriesForContentType(ContentfulClient client, ExportModel model, string contentType, Type dtoType)
        {
            var skip = 0;
            const int batchSize = 80;

            var allEntries = new List<object>();

            do
            {
                var queryBuilderType = typeof(QueryBuilder<>).MakeGenericType(dtoType); // Create a generic type using reflection
                var queryBuilder = Activator.CreateInstance(queryBuilderType); // Create an instance of the generic type

                // Use reflection to invoke the methods on the queryBuilder object
                queryBuilderType.GetMethod("ContentTypeIs").Invoke(queryBuilder, new object[] { contentType });
                queryBuilderType.GetMethod("LocaleIs").Invoke(queryBuilder, new object[] { model.Locale });
                queryBuilderType.GetMethod("Skip").Invoke(queryBuilder, new object[] { skip });
                queryBuilderType.GetMethod("Include").Invoke(queryBuilder, new object[] { 2 });
                queryBuilderType.GetMethod("Limit").Invoke(queryBuilder, new object[] { batchSize });

                var pageEntries = await client.GetEntries((dynamic)queryBuilder); // Cast the queryBuilder object to dynamic

                // Add the pageEntries to allEntries
                allEntries.AddRange(pageEntries);


                skip += batchSize;
            } while (allEntries.Count % batchSize == 0);

            return allEntries;
        }

        public static void ExportToExcel(Dictionary<string, IEnumerable<object>> data, string path)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                foreach (var contentType in data.Keys)
                {
                    var collection = data[contentType];

                    // Check if the collection is empty
                    if (!collection.Any())
                        continue;

                    // Get the type of the first object in the collection
                    var type = collection.First().GetType();

                    // Convert the collection to a list of the specific type
                    var typedCollection = typeof(Enumerable)
                        .GetMethod("Cast")
                        .MakeGenericMethod(type)
                        .Invoke(null, new object[] { collection });

                    var ws = package.Workbook.Worksheets.Add(contentType);
                    ws.Cells["A1"].LoadFromCollection((dynamic)typedCollection, true);
                }
                package.SaveAs(new FileInfo(path));
            }
        }

       


    }
}
