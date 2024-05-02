using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Search;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ContentfulApp.Models.DTO;
using OfficeOpenXml;
using ContentfulApp.Models.DTO.ExportDto;
using System.Drawing;
using Contentful.Core.Models;
using ContentfulApp.Services;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {
        private readonly IContentfulService _contentfulService;
        private readonly IDtoMappingService _dtoMappingService;
        private readonly IExcelExportService _excelExportService;

        public ExportController(IContentfulService contentfulService, IDtoMappingService dtoMappingService, IExcelExportService excelExportService)
        {
            _contentfulService = contentfulService;
            _dtoMappingService = dtoMappingService;
            _excelExportService = excelExportService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Index(ExportModel model)
        {
            var contentTypes = ParseContetTypes(model.ContentTypes);
            var locales = ParseLocales(model.Locales);
            var client = await _contentfulService.GetClientAsync(model.AccessToken, model.SpaceId, model.Environment);

            var allEntries = await GetAllEntries(contentTypes, locales, client);

            var excelfilePath = GenerateExcelFile(allEntries, model.Environment);

            return Content(excelfilePath);
        }

        private List<string> ParseContetTypes(string contentTypes)
        {
            return contentTypes.Split(',').Select(c => c.Trim()).ToList();
        }

        private List<string> ParseLocales(string locales)
        {
            return locales.Split(',').Select(c => c.Trim()).ToList();
        }

        private async Task<Dictionary<string, IEnumerable<object>>> GetAllEntries(List<string> contentTypes, List<string> locales, ContentfulClient client)
        {
            var allEntries = new Dictionary<string, IEnumerable<object>>();
            foreach (var contentType in contentTypes)
            {
                foreach (var locale in locales)
                {
                    var entries = await GetEntriesForContentTypeAndLocale(client, contentType, locale);
                    var key = $"{contentType}-{locale}";
                    allEntries.Add(key, entries);
                }
            }
            return allEntries;
        }

        private async Task<IEnumerable<object>> GetEntriesForContentTypeAndLocale(ContentfulClient client, string contentType, string locale)
        {
            var dtoType = GetDtoType(contentType);
            var entries = await _contentfulService.GetEntriesForContentType(client, contentType, dtoType, locale);
            
            var exportDtoType = GetExportDtoType(contentType);
            return entries.Select(e => _dtoMappingService.MapToExportDto(e, exportDtoType));
        }

        private Type GetDtoType(string contentType)
        {
            return contentType switch
            {
                "productListingPage" => typeof(FullEntryDto),
                "brand" => typeof(FullEntryDto),
                "collection" => typeof(FullEntryDto),
                "designer" => typeof(FullEntryDto),
                _ => typeof(RegularEntryDto)
            };
        }

        private Type GetExportDtoType(string contentType)
        {
            if (!ContentTypeToExportDtoTypeMap.TryGetValue(contentType, out var exportDtoType))
            {
                exportDtoType = ContentTypeToExportDtoTypeMap["_default"];
            }

            return exportDtoType;
        }

        private string GenerateExcelFile(Dictionary<string,IEnumerable<object>> allentries, string environment)
        {
            var environmentName = environment == "master" ? "" : environment;
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

            var excelFileName = $"export-{environmentName}-{currentDateTime}.xlsx";
            var excelfilePath = Path.Combine(Path.GetTempPath(), excelFileName);

            _excelExportService.ExportToExcel(allentries, excelfilePath);

            return excelfilePath;
        }

        public static Dictionary<string, Type> ContentTypeToExportDtoTypeMap = new Dictionary<string, Type>
        {
            { "productListingPage", typeof(FullExport) },
            { "brand", typeof(FullExport) },
            { "collection", typeof(FullExport) },
            { "designer", typeof(FullExport) },
            { "_default", typeof(RegularExport) }
        };

        private async Task<string> GetEntriesAsJson(ExportModel model)
        {
            var client = await _contentfulService.GetClientAsync(model.AccessToken, model.SpaceId, model.Environment);

            QueryBuilder<dynamic> queryBuilder = new QueryBuilder<dynamic>();
            queryBuilder.ContentTypeIs("productListingPage");
            queryBuilder.Limit(10);
            queryBuilder.Include(4);
            var entries = await client.GetEntries<dynamic>(queryBuilder);

            //entries as json
            var entriesAsJson = JsonConvert.SerializeObject(entries);
            return entriesAsJson;
        }
    }
}
