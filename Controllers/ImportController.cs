using Contentful.Core;
using Contentful.Core.Errors;
using Contentful.Core.Models;
using Contentful.Core.Models.Management;
using Contentful.Core.Search;
using ContentfulApp.Models;
using ContentfulApp.Models.DTO;
using ContentfulApp.Models.DTO.ExportDto;
using ContentfulApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using static Contentful.RichTextParser.Parser;
using System.Xml.Linq;
using System.Globalization;

namespace ContentfulApp.Controllers
{
    public class ImportController : Controller
    {
        private readonly IContentfulManagementClient _client;
        private readonly IExcelImportService _excelImportService; 
        private readonly IContentfulService _contentfulService;
        private readonly IDtoMappingService _dtoMappingService; 

        public async Task<IActionResult> Index()
        {
            return View();
        }


        public ImportController(IContentfulManagementClient client, IContentfulService contentfulService, IExcelImportService excelImportServide, IDtoMappingService dtoMappingService)
        {
            _client = client;
            _contentfulService = contentfulService;
            _excelImportService = excelImportServide;
            _dtoMappingService = dtoMappingService;
        }

        
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, ImportModel import)
        {
            var managementClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);

            // Use EPPlus to read the Excel file
            var data = _excelImportService.ImportFromExcel(file);
            bool anyUpdates = false;

            foreach (var sheet in data)
            {
                var dataTable = sheet.Value;

                // Extract header row
                var headerRow = dataTable.Rows[0];
                var headers = headerRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

                var name = sheet.Key.Split(",");
                var contentType = name[0];
                var locale = name[1].Replace(" ", "");

                // For each row in the Excel file, update the corresponding content in Contentful
                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {
                    var row = dataTable.Rows[rowIndex];
                    var id = row[0].ToString(); // Assuming the ID is in the first column

                    var exportDtoType = _dtoMappingService.GetExportDtoType(contentType);

                    if (exportDtoType == typeof(FullExport))
                    {
                        var newEntry = MapRowToFullExport(row, headers);

                        //var fullEntryDto = MapFullExportToDto(newEntry);

                        // Get the existing entry with the current version
                        var getEntry = await managementClient.GetEntry(id);
                       
                        var currentEntry = MapFullEntryToDto(getEntry, locale);
                        
                        var fullCurrentEntry = (FullExport)_dtoMappingService.MapToExportDto(currentEntry, typeof(FullExport));

                        bool isUpdated = false;

                        foreach (PropertyInfo property in typeof(FullExport).GetProperties())
                        {
                            if (property.Name == "CreatedAt")
                            {
                                var currentValue = property.GetValue(fullCurrentEntry);
                                var newValue = property.GetValue(newEntry);

                                if (currentValue != null && newValue != null)
                                {
                                    // Convert DateTime to string in "yyyy-MM-dd" format
                                    string currentDateString = ((DateTime)currentValue).ToString("yyyy-MM-dd");
                                    string newDateString = ((DateTime)newValue).ToString("yyyy-MM-dd");

                                    // Compare dates
                                    if (!currentDateString.Equals(newDateString))
                                    {
                                        // If dates are different, update the date and set the time to 00:00
                                        DateTime newDate = DateTime.Parse(newDateString);
                                        property.SetValue(fullCurrentEntry, newDate);
                                        isUpdated = true;
                                    }
                                }
                            }
                            else
                            {
                                var currentValue = property.GetValue(fullCurrentEntry);
                                var newValue = property.GetValue(newEntry);

                                if (newValue != null && !newValue.Equals(currentValue))
                                {
                                    property.SetValue(fullCurrentEntry, newValue);
                                    isUpdated = true;
                                }
                            }
                        }

                        var newEntrytoUpdate = MapFullExportToDto(fullCurrentEntry);
                        
                        if (isUpdated)
                        {
                            // Update the entry
                            var updatedEntry = await managementClient.UpdateEntryForLocale(newEntrytoUpdate, id, locale);

                            await Task.Delay(2000);

                            if (import.PublishChanges)
                            {
                                var publishedEntry = await managementClient.PublishEntry(id, (int)updatedEntry.SystemProperties.Version);
                            }
                            anyUpdates = true;
                        }
                        
                    }
                    else
                    {
                        var newEntry = MapRowToRegularExport(row, headers);

                        // Get the existing entry with the current version
                        var getEntry = await managementClient.GetEntry(id);
                        var currentEntry = MapRegularEntryToDto(getEntry, locale);

                        var regularCurrentEntry = (RegularExport)_dtoMappingService.MapToExportDto(currentEntry, typeof(RegularExport));

                        bool isUpdated = false;

                        foreach (PropertyInfo property in typeof(RegularExport).GetProperties())
                        {
                            if (property.Name == "CreatedAt")
                            {
                                var currentValue = property.GetValue(regularCurrentEntry);
                                var newValue = property.GetValue(newEntry);

                                if (currentValue != null && newValue != null)
                                {
                                    // Convert DateTime to string in "yyyy-MM-dd" format
                                    string currentDateString = ((DateTime)currentValue).ToString("yyyy-MM-dd");
                                    string newDateString = ((DateTime)newValue).ToString("yyyy-MM-dd");

                                    // Compare dates
                                    if (!currentDateString.Equals(newDateString))
                                    {
                                        // If dates are different, update the date and set the time to 00:00
                                        DateTime newDate = DateTime.Parse(newDateString);
                                        property.SetValue(regularCurrentEntry, newDate);
                                        isUpdated = true;
                                    }
                                }
                            }
                            else
                            {
                                var currentValue = property.GetValue(regularCurrentEntry);
                                var newValue = property.GetValue(newEntry);

                                if (newValue != null && !newValue.Equals(currentValue))
                                {
                                    property.SetValue(regularCurrentEntry, newValue);
                                    isUpdated = true;
                                }
                            }
                        }
                        var newEntrytoUpdate = MapRegularExportToDto(regularCurrentEntry);

                        if (isUpdated)
                        {
                            // Update the entry
                            var updatedEntry = await managementClient.UpdateEntryForLocale(newEntrytoUpdate, id, locale);

                            await Task.Delay(2000);

                            if (import.PublishChanges)
                            {
                                var publishedEntry = await managementClient.PublishEntry(id, (int)updatedEntry.SystemProperties.Version);
                            }
                            
                            anyUpdates = true;
                        }

                    }

                }
            }
            if (anyUpdates)
            {
                return Ok("All entries updated successfully.");
            }
            else
            {
                return Ok("There were no entries to update in this Excelfile.");
            }
        }



        private RegularEntryDto MapRegularEntryToDto(Entry<dynamic> entry,string locale)
        {
            var dto = new RegularEntryDto()
            {
                Sys = entry.SystemProperties,
                InternalName = entry.Fields.ContainsKey("internalName") && entry.Fields["internalName"][locale] != null ? entry.Fields["internalName"][locale].ToString() : null,
                Name = entry.Fields.ContainsKey("name") ? entry.Fields["name"][locale].ToString() : null,
                Slug = entry.Fields.ContainsKey("slug") ? entry.Fields["slug"][locale].ToString() : null,
                Urls = entry.Fields.ContainsKey("urls") ? JsonConvert.DeserializeObject<List<List<string>>>(entry.Fields["urls"][locale].ToString()) : null,
                Metadata = entry.Fields.ContainsKey("$metadata") ? JsonConvert.DeserializeObject<ContentfulMetadata>(entry.Fields["$metadata"][locale].ToString()) : null
            };

            return dto;
        }

        private FullEntryDto MapFullEntryToDto(Entry<dynamic> entry, string locale)
        {
            var dto = new FullEntryDto()
            {
                Sys = entry.SystemProperties,
                InternalName = entry.Fields.ContainsKey("internalName") && entry.Fields["internalName"][locale] != null ? entry.Fields["internalName"][locale].ToString() : null,
                Name = entry.Fields.ContainsKey("name") && entry.Fields["name"][locale] != null ? entry.Fields["name"][locale].ToString() : null,
                IsPrimaryCategory = entry.Fields.ContainsKey("isPrimaryCategory") && entry.Fields["isPrimaryCategory"][locale] != null ? bool.Parse(entry.Fields["isPrimaryCategory"][locale].ToString()) : false,
                CategoryRank = entry.Fields.ContainsKey("categoryRank") && entry.Fields["categoryRank"][locale] != null ? int.Parse(entry.Fields["categoryRank"][locale].ToString()) : 0,
                ShortDescription = entry.Fields.ContainsKey("shortDescription") && entry.Fields["shortDescription"][locale] != null ? entry.Fields["shortDescription"][locale].ToString() : null,
                Filter = entry.Fields.ContainsKey("filter") && entry.Fields["filter"][locale] != null ? JsonConvert.DeserializeObject<Filter>(entry.Fields["filter"][locale].ToString()) : null,
                Active = entry.Fields.ContainsKey("active") && entry.Fields["active"][locale] != null ? JsonConvert.DeserializeObject<List<string>>(entry.Fields["active"][locale].ToString()) : null,
                CreateLinksOnProductPages = entry.Fields.ContainsKey("createLinksOnProductPages") && entry.Fields["createLinksOnProductPages"][locale] != null ? bool.Parse(entry.Fields["createLinksOnProductPages"][locale].ToString()) : false,
                UseAsFacet = entry.Fields.ContainsKey("useAsFacet") && entry.Fields["useAsFacet"][locale] != null ? bool.Parse(entry.Fields["useAsFacet"][locale].ToString()) : false,
                SeoInfo = entry.Fields.ContainsKey("seoInfo") && entry.Fields["seoInfo"][locale] != null ? JsonConvert.DeserializeObject<SeoInfo>(entry.Fields["seoInfo"][locale].ToString()) : null,
                Metadata = entry.Fields.ContainsKey("$metadata") && entry.Fields["$metadata"][locale] != null ? JsonConvert.DeserializeObject<ContentfulMetadata>(entry.Fields["$metadata"][locale].ToString()) : null,
                //Facets = entry.Fields.ContainsKey("facets") && entry.Fields["facets"][locale] != null ? JsonConvert.DeserializeObject<List<string>>(entry.Fields["facets"][locale].ToString()) : null,
                Slug = entry.Fields.ContainsKey("slug") && entry.Fields["slug"][locale] != null ? entry.Fields["slug"][locale].ToString() : null,
                Urls = entry.Fields.ContainsKey("urls") && entry.Fields["urls"][locale] != null ? JsonConvert.DeserializeObject<List<List<string>>>(entry.Fields["urls"][locale].ToString()) : null,
                //AdditionalContentDescription = entry.Fields.ContainsKey("additionalContentDescription") && entry.Fields["additionalContentDescription"][locale] != null ? JsonConvert.DeserializeObject<Document>(entry.Fields["additionalContentDescription"][locale].ToString()) : null,
                H1Title = entry.Fields.ContainsKey("h1Title") && entry.Fields["h1Title"][locale] != null ? entry.Fields["h1Title"][locale].ToString() : null
            };
            return dto;
        }


        private RegularExport MapRowToRegularExport(DataRow row, List<string> headers)
        {
            var regularExport = new RegularExport();

            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                var value = row[i].ToString();

                var property = typeof(RegularExport).GetProperty(header, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        property.SetValue(regularExport, null);
                    }
                    else if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                    {
                        property.SetValue(regularExport, intValue);
                    }
                    else if (property.PropertyType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                    {
                        property.SetValue(regularExport, boolValue);
                    }
                    else if (property.PropertyType == typeof(DateTime?) && DateTime.TryParse(value, out DateTime dateValue))
                    {
                        property.SetValue(regularExport, dateValue);
                    }
                    else
                    {
                        property.SetValue(regularExport, value);
                    }
                }
            }

            return regularExport;
        }

        private FullExport MapRowToFullExport(DataRow row, List<string> headers)
        {
            var fullExport = new FullExport();

            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                var value = row[i].ToString();

                var property = typeof(FullExport).GetProperty(header, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        property.SetValue(fullExport, null);
                    }
                    else if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                    {
                        property.SetValue(fullExport, intValue);
                    }
                    else if (property.PropertyType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                    {
                        property.SetValue(fullExport, boolValue);
                    }
                    else if (property.PropertyType == typeof(DateTime?) && DateTime.TryParse(value, out DateTime dateValue))
                    {
                        property.SetValue(fullExport, dateValue);
                    }
                    else
                    {
                        property.SetValue(fullExport, value);
                    }
                }
            }

            return fullExport;
        }

        private RegularEntryDto MapRegularExportToDto(RegularExport regularExport)
        {
            var regularEntryDto = new RegularEntryDto();
            
            regularEntryDto.Sys = new SystemProperties { Id = regularExport.Id, CreatedAt = regularExport.CreatedAt };
            regularEntryDto.InternalName = regularExport.InternalName;
            regularEntryDto.Name = regularExport.Name;
            regularEntryDto.Slug = regularExport.Slug;
            regularEntryDto.Urls = new List<List<string>> { new List<string> { regularExport.Urls } };
            if (regularExport.Tags != null)
            {
                regularEntryDto.Metadata = new ContentfulMetadata
                {
                    Tags = regularExport.Tags.Split(',').Select(tag => new Reference { Sys = new ReferenceProperties { Id = tag } }).ToList()
                };
            }

            return regularEntryDto;
        }

        private FullEntryDto MapFullExportToDto(FullExport fullExport)
        {
            var fullEntryDto = new FullEntryDto();

            fullEntryDto.Sys = new SystemProperties { Id = fullExport.Id, CreatedAt = fullExport.CreatedAt };
            fullEntryDto.InternalName = fullExport.InternalName;
            fullEntryDto.Name = fullExport.Name;
            fullEntryDto.IsPrimaryCategory = fullExport.IsPrimaryCategory;
            fullEntryDto.CategoryRank = fullExport.CategoryRank;
            fullEntryDto.ShortDescription = fullExport.ShortDescription;
            fullEntryDto.Filter = new Filter { _rawFilterData = fullExport.Filter };
            fullEntryDto.CreateLinksOnProductPages = fullExport.CreateLinksOnProductPages;
            fullEntryDto.UseAsFacet = fullExport.UseAsFacet;
            //fullEntryDto.AdditionalContentDescription = new Document { Content = newEntry.AdditionalContentDescription };
            fullEntryDto.Active = new List<string> { fullExport.Active.ToString() };
            fullEntryDto.H1Title = fullExport.H1Title;
            fullEntryDto.Slug = fullExport.Slug;
            fullEntryDto.Urls = new List<List<string>> { new List<string> { fullExport.Urls } };
            //fullEntryDto.Facets = new List<string> { fullExport.Facets };
            //fullEntryDto.SeoInfo = new SeoInfo { Title = fullExport.SeoTitle, Description = fullExport.SeoDescription };
            //tags och metadata?? 

            return fullEntryDto;
        }
    }
}
