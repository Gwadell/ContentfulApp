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
using Contentful.Core.Models.Management;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Concurrent;
using Polly;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {
        private readonly IContentfulService _contentfulService;
        private readonly IExcelExportService _excelExportService;
        private readonly IContentfulManagementClient _managementClient;
        private readonly ILogger<ExportController> _logger;


        public ExportController(IContentfulService contentfulService, ILogger<ExportController> logger, IExcelExportService excelExportService, IContentfulManagementClient managementClient)
        {
            _contentfulService = contentfulService;
            _logger = logger;
            _excelExportService = excelExportService;
            _managementClient = managementClient;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> Index(ExportModel model)
        {
            try
            {
                var contentTypes = ParseContetTypes(model.ContentTypesId);
                var locales = ParseLocales(model.Locales);
                var client = await _contentfulService.GetClientAsync(model.AccessToken, model.SpaceId, model.Environment);

                var allEntries = await GetAllEntries(contentTypes, locales, client);

                var excelfilePath = _excelExportService.GenerateExcelFile(allEntries, model.Environment);

                return Content(excelfilePath);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Parses the given content types string into a list of content types.
        /// </summary>
        /// <param name="contentTypes">The content types string to parse.</param>
        /// <returns>A list of parsed content types.</returns>
        private List<string> ParseContetTypes(string contentTypes)
        {
            if (string.IsNullOrEmpty(contentTypes))
            {
                throw new ArgumentException("Content types cannot be null or empty.");
            }

            return contentTypes.Split(',').Select(c => c.Trim()).ToList();
        }

        /// <summary>
        /// Parses the given locales string into a list of locales.
        /// </summary>
        /// <param name="locales">The locales string to parse</param>
        /// <returns>A list of parsed locales</returns>
        private List<string> ParseLocales(string locales)
        {
            if (string.IsNullOrEmpty(locales))
            {
                throw new ArgumentException("Locales cannot be null or empty.");
            }

            return locales.Split(',').Select(c => c.Trim()).ToList();
        }

        /// <summary>
        /// Retrieves all entries from Contentful for the given content types and locales.
        /// </summary>
        /// <param name="contentTypes">The list of content types to retrieve entries for.</param>
        /// <param name="locales">The list of locales to retrieve entries for.</param>
        /// <param name="client">The Contentful client.</param>
        /// <returns>A dictionary containing the retrieved entries, with the key being a combination of content type and locale.</returns>
        //private async Task<Dictionary<string, IEnumerable<object>>> GetAllEntries(List<string> contentTypes, List<string> locales, ContentfulClient client)
        //{
        //    var allEntries = new Dictionary<string, IEnumerable<object>>();
        //    foreach (var contentType in contentTypes)
        //    {
        //        foreach (var locale in locales)
        //        {
        //            var entries = await _contentfulService.GetEntriesForContentTypeAndLocale(client, contentType, locale);
        //            var key = $"{contentType}-{locale}";
        //            allEntries.Add(key, entries);
        //        }
        //    }
        //    return allEntries;
        //}

        private async Task<Dictionary<string, IEnumerable<object>>> GetAllEntries(List<string> contentTypes, List<string> locales, ContentfulClient client)
        {
            var allEntries = new ConcurrentDictionary<string, IEnumerable<object>>();
            var tasks = new List<Task>();

            var retryPolicy = Polly.Policy
                .Handle<IOException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, context) =>
                {
                    _logger.LogWarning($"An IOException occurred. Retrying in {timeSpan.Seconds} seconds.");
                });

            foreach (var contentType in contentTypes)
            {
                foreach (var locale in locales)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            var entries = await _contentfulService.GetEntriesForContentTypeAndLocale(client, contentType, locale);
                            var key = $"{contentType}-{locale}";
                            allEntries.TryAdd(key, entries);
                            _logger.LogInformation($"Completed fetching entries for content type: {contentType} and locale: {locale}");
                        });
                    }));
                }
            }

            await Task.WhenAll(tasks);
            return new Dictionary<string, IEnumerable<object>>(allEntries);
        }


        private async Task<string> GetEntriesAsJson(ExportModel model)
        {
            var client = await _contentfulService.GetClientAsync(model.AccessToken, model.SpaceId, model.Environment);

            QueryBuilder<dynamic> queryBuilder = new QueryBuilder<dynamic>();
            queryBuilder.ContentTypeIs("productListingPage");
            queryBuilder.LocaleIs("sv-SE");
            queryBuilder.Limit(10);
            queryBuilder.Include(2);
            var entries = await client.GetEntries<dynamic>(queryBuilder);

            var entriesAsJson = JsonConvert.SerializeObject(entries);
            return entriesAsJson;
        }

        //private async Task<IEnumerable<FullEntryDto>> GetManagementEntries(ExportModel model)
        //{
        //    var entries = await _managementClient.GetEntriesCollection<dynamic>();

        //    return entries;
        //}

        [HttpGet]
        public async Task<IActionResult> GetLocales(string accessToken, string environment, string spaceId)
        {
            var locales = await _contentfulService.GetLocales(accessToken, environment, spaceId);

            var selectListItems = locales.Select(l => new SelectListItem
            {
                Value = l.Code,
                Text = l.Name
            });

            return Json(selectListItems);
        }


    }
}
