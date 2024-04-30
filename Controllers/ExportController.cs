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
using System.Reflection;
using Contentful.Core.Models.Management;
using ContentfulApp.Models.DTO.ExportDto;
using System.Drawing;

namespace ContentfulApp.Controllers
{
    public class ExportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        public ContentfulClient GetContentfulClient(ExportModel model)
        {
            var httpClient = new HttpClient();
            var options = new ContentfulOptions
            {
                DeliveryApiKey = model.AccessToken,
                SpaceId = model.SpaceId,
                Environment = model.Environment
            };
            return new ContentfulClient(httpClient, options);
        }

        [HttpPost]
        public async Task<ActionResult> Index(ExportModel model)
        {
            var contentTypes = model.ContentTypes.Split(',').Select(c => c.Trim()).ToList();
            var locales = model.Locales.Split(',').Select(c => c.Trim()).ToList();
            var client = GetContentfulClient(model);

            QueryBuilder<dynamic> queryBuilder = new QueryBuilder<dynamic>();
            queryBuilder.ContentTypeIs("productListingPage");
            queryBuilder.Limit(20);
            queryBuilder.Include(2);
            var entries1 = await client.GetEntries<dynamic>(queryBuilder);

            //entries as json
            var entriesAsJson = JsonConvert.SerializeObject(entries1);

            //var getEntryByID = await client.GetEntry<dynamic>("6hUtLDGZGk34LDA2GlnSPV"); 

            //var convertEntryToJSON = JsonConvert.SerializeObject(getEntryByID);

            var allEntries = new Dictionary<string, IEnumerable<object>>();

            foreach (var contentType in contentTypes)
            {
                
                foreach (var locale in locales)
                {
                    var dtoType = contentType switch
                    {
                        "productListingPage" => typeof(EntryPlpDto),
                        "brand" => typeof(EntryPlpDto),
                        "collection" => typeof(EntryPlpDto),
                        "designer" => typeof(EntryPlpDto),
                        _ => typeof(EntryBrandDto)
                    };

                    var entries = await GetEntriesForContentType(client, model, contentType, dtoType, locale);

                    if (!ContentTypeToExportDtoTypeMap.TryGetValue(contentType, out var exportDtoType))
                    {
                        exportDtoType = ContentTypeToExportDtoTypeMap["_default"];
                    }
                    //var exportDtoType = ContentTypeToExportDtoTypeMap[contentType];

                    var exportEntries = entries.Select(e => MapToExportDto(e, exportDtoType)); // Use the mapping function
                    var key = $"{contentType}-{locale}";
                    allEntries.Add(key, exportEntries);
                }
            }

            var environmentName = model.Environment == "master" ? "" : model.Environment;
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

            var excelFileName = $"export-{environmentName}-{currentDateTime}.xlsx";
            var excelfilePath = Path.Combine(Path.GetTempPath(), excelFileName);

            ExportToExcel(allEntries, excelfilePath);

            return Content(excelfilePath);
        }

        private async Task<IEnumerable<object>> GetEntriesForContentType(ContentfulClient client, ExportModel model, string contentType, Type dtoType, string locale)
        {
            var skip = 0;
            const int batchSize = 10;

            var allEntries = new List<object>();

            do
            {
                var queryBuilderType = typeof(QueryBuilder<>).MakeGenericType(dtoType); // Create a generic type using reflection
                var queryBuilder = Activator.CreateInstance(queryBuilderType); // Create an instance of the generic type

                queryBuilderType.GetMethod("ContentTypeIs").Invoke(queryBuilder, new object[] { contentType });
                queryBuilderType.GetMethod("LocaleIs").Invoke(queryBuilder, new object[] { locale});
                queryBuilderType.GetMethod("Skip").Invoke(queryBuilder, new object[] { skip });
                queryBuilderType.GetMethod("Include").Invoke(queryBuilder, new object[] { 0 });
                queryBuilderType.GetMethod("Limit").Invoke(queryBuilder, new object[] { batchSize });

                var pageEntries = await client.GetEntries((dynamic)queryBuilder);

                
                //var pageEntriesJson = JsonConvert.SerializeObject(pageEntries);


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
                foreach (var sheet in data.Keys)
                {
                    var collection = data[sheet];

                    if (!collection.Any())
                        continue;

                    var type = collection.First().GetType();

                    // Convert the collection to a list of the specific type
                    var typedCollection = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(type).Invoke(null, new object[] { collection });

                    var ws = package.Workbook.Worksheets.Add(sheet);
                    ws.Cells["A1"].LoadFromCollection((dynamic)typedCollection, true);


                    // Format header row
                    var headerRange = ws.Cells[1, 1, 1, ws.Dimension.End.Column];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    // Autofit columns
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    // Format CreatedAt column as date
                    var createdAtColumn = ws.Cells[1, 1, 1, ws.Dimension.End.Column].FirstOrDefault(c => c.Text == "CreatedAt");
                    if (createdAtColumn != null)
                    {
                        var columnNumber = createdAtColumn.Start.Column;
                        var columnRange = ws.Cells[2, columnNumber, ws.Dimension.End.Row, columnNumber];
                        columnRange.Style.Numberformat.Format = "yyyy-mm-dd-hh-ss";

                    }

                }
                package.SaveAs(new FileInfo(path));
            }
        }

        public static Dictionary<string, Type> ContentTypeToExportDtoTypeMap = new Dictionary<string, Type>
        {
            { "productListingPage", typeof(EntryPlpDtoExport) },
            { "brand", typeof(EntryPlpDtoExport) },
            { "collection", typeof(EntryPlpDtoExport) },
            { "designer", typeof(EntryPlpDtoExport) },
            { "_default", typeof(EntryBrandDtoExport) }
        };

        private object MapToExportDto(object dto, Type exportDtoType)
        {
            switch (exportDtoType.Name)
            {
                case nameof(EntryPlpDtoExport) when dto is EntryPlpDto entryPlpDto:
                    return new EntryPlpDtoExport
                    {
                        Id = entryPlpDto.Sys.Id,
                        InternalName = entryPlpDto.InternalName,
                        Name = entryPlpDto.Name,
                        IsPrimaryCategory = entryPlpDto.IsPrimaryCategory,
                        CategoryRank = entryPlpDto.CategoryRank,
                        ShortDescription = entryPlpDto.ShortDescription,
                        Filter = entryPlpDto.Filter?._rawFilterData,
                        //SubPageData = entryPlpDto.SubPageData.Routes,
                        AdditionalContentDescription = entryPlpDto.GetAdditionalContentDescriptionAsString(), //funkar ej 
                        Active = entryPlpDto.IsActiveLocale(),
                        CreateLinksOnProductPages = entryPlpDto.CreateLinksOnProductPages,
                        UseAsFacet = entryPlpDto.UseAsFacet,
                        Tags = entryPlpDto.GetTagsAsString(), //funkar ej 
                        Facets = entryPlpDto.GetFacetsAsString(),
                        SeoTitle = entryPlpDto.SeoInfo?.Title,
                        SeoDescription = entryPlpDto.SeoInfo?.Description,
                        Slug = entryPlpDto.Slug,
                        CreatedAt = entryPlpDto.Sys.CreatedAt,
                        Urls = entryPlpDto.GetLastUrl(),
                        Archived = entryPlpDto.IsArchived(),
                        H1Title = entryPlpDto.H1Title,
                        //archived
                    };
                case nameof(EntryBrandDtoExport) when dto is EntryBrandDto entryBrandDto:
                    return new EntryBrandDtoExport
                    {
                        Id = entryBrandDto.Sys.Id,
                        InternalName = entryBrandDto.InternalName,
                        Name = entryBrandDto.Name,
                        Slug = entryBrandDto.Slug,
                        Urls = entryBrandDto.GetLastUrl(),
                        CreatedAt = entryBrandDto.Sys.CreatedAt,
                        //archived
                        //tags
                    };
                default:
                    return null;
                    
                }
        
        }
    }
}
