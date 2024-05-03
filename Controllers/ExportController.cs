﻿using Contentful.Core;
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
            var contentTypes = ParseContetTypes(model.ContentTypesId);
            var locales = ParseLocales(model.Locales);
            var client = await _contentfulService.GetClientAsync(model.AccessToken, model.SpaceId, model.Environment);

            var allEntries = await GetAllEntries(contentTypes, locales, client);

            var excelfilePath = _excelExportService.GenerateExcelFile(allEntries, model.Environment);

            return Content(excelfilePath);
        }

        /// <summary>
        /// Parses the given content types string into a list of content types.
        /// </summary>
        /// <param name="contentTypes">The content types string to parse.</param>
        /// <returns>A list of parsed content types.</returns>
        private List<string> ParseContetTypes(string contentTypes)
        {
            return contentTypes.Split(',').Select(c => c.Trim()).ToList();
        }

        /// <summary>
        /// Parses the given locales string into a list of locales.
        /// </summary>
        /// <param name="locales">The locales string to parse</param>
        /// <returns>A list of parsed locales</returns>
        private List<string> ParseLocales(string locales)
        {
            return locales.Split(',').Select(c => c.Trim()).ToList();
        }

        /// <summary>
        /// Retrieves all entries from Contentful for the given content types and locales.
        /// </summary>
        /// <param name="contentTypes">The list of content types to retrieve entries for.</param>
        /// <param name="locales">The list of locales to retrieve entries for.</param>
        /// <param name="client">The Contentful client.</param>
        /// <returns>A dictionary containing the retrieved entries, with the key being a combination of content type and locale.</returns>
        private async Task<Dictionary<string, IEnumerable<object>>> GetAllEntries(List<string> contentTypes, List<string> locales, ContentfulClient client)
        {
            var allEntries = new Dictionary<string, IEnumerable<object>>();
            foreach (var contentType in contentTypes)
            {
                foreach (var locale in locales)
                {
                    var entries = await _contentfulService.GetEntriesForContentTypeAndLocale(client, contentType, locale);
                    var key = $"{contentType}-{locale}";
                    allEntries.Add(key, entries);
                }
            }
            return allEntries;
        }


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
