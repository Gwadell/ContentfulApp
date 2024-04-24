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

            List<EntryPlpDto> allEntries = new List<EntryPlpDto>();

            do
            {
                var queryBuilder = new QueryBuilder<EntryPlpDto>();

                queryBuilder.ContentTypeIs(model.ContentType);
                queryBuilder.LocaleIs(model.Locale);
                queryBuilder.Skip(skip);
                queryBuilder.Include(2);
                queryBuilder.Limit(batchSize);

                var pageEntries = await client.GetEntries(queryBuilder);

                allEntries.AddRange(pageEntries);

                skip += batchSize;
            } while (allEntries.Count % batchSize == 0);

            var environmentName = model.Environment == "master" ? "" : model.Environment;
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm"); 

            var excelFileName = $"export-{environmentName}-{currentDateTime}.xlsx";
            var sheetName = $"{model.ContentType}-{model.Locale}";

            var excelfilePath = Path.Combine(Path.GetTempPath(), excelFileName);

            ExportToExcel(allEntries, excelfilePath, sheetName);

            return Content(excelfilePath);
        }

        

        public static void ExportToExcel<T>(IEnumerable<T> data, string path, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add(sheetName);
                ws.Cells["A1"].LoadFromCollection(data, true);
                package.SaveAs(new FileInfo(path));
            }
        }

        
    }
}
