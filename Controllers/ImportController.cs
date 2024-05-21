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

        public async Task<IActionResult> Index()
        {
            //var publishedEntry = await PublishEntrys(new ImportModel());

            return View();
        }


        public ImportController(IContentfulManagementClient client, IContentfulService contentfulService, IExcelImportService excelImportServide)
        {
            _client = client;
            _contentfulService = contentfulService;
            _excelImportService = excelImportServide;
        }

        private async Task<Entry<dynamic>> PublishEntrys(ImportModel import)
        {
            try
            {
                var managementClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);
                var id = "sP0pnINXLFs7ZuiSut5da";

                var entry = await managementClient.GetEntry(id);

                if (entry == null)
                {
                    Console.WriteLine($"{id} not found.");
                    return null;
                }

                // Publish the entry
                var publishedEntry = await managementClient.PublishEntry(id, (int)entry.SystemProperties.Version);

                return publishedEntry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" {ex.Message}");
                return null;
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> Import(IFormFile file, ImportModel import)
        //{
        //    var managementClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);

        //    // Use EPPlus to read the Excel file
        //    var data = _excelImportService.ImportFromExcel(file);

        //    foreach (var sheet in data)
        //    {
        //        var name = sheet.Key.Split(",");
        //        var contentType = name[0];
        //        var locale = name[1].Replace(" ", "");

        //        var dataTable = sheet.Value;

        //        //For each row in the Excel file, update the corresponding content in Contentful
        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            var id = row[0].ToString(); // Assuming the ID is in the first column

        //            var newName = row[1].ToString(); // Assuming the new name is in the second column

        //            // Get the existing entry with the current version
        //            var entry = await managementClient.GetEntry(id);

        //            // If the name field is null, skip this row and continue with the next one
        //            if (entry.Fields["name"][locale].ToString() == null){ continue;}

        //            var currentName = entry.Fields["name"][locale].ToString();

        //            // If the name field is null or does not exist, skip this row and continue with the next one
        //            if (entry.Fields.ContainsKey("name") && entry.Fields["name"].ContainsKey(locale) && entry.Fields["name"][locale] != null)
        //            {


        //                if (newName != currentName)
        //                {
        //                    var regularEntryDto = new RegularEntryDto();
        //                    regularEntryDto.Name = newName;

        //                    var version = entry.SystemProperties.Version;

        //                    if (entry.SystemProperties.Version == entry.SystemProperties.PublishedVersion)
        //                    {
        //                        var unpublishedEntry = await managementClient.UnpublishEntry(id, (int)version);
        //                    }

        //                    var updatedEntry = await managementClient.UpdateEntryForLocale(regularEntryDto, id, locale);

        //                    // Get the updated version number
        //                    var updatedVersion = updatedEntry.SystemProperties.Version;
        //                    Console.WriteLine($"Entry ID: {entry.SystemProperties.Id}");
        //                    Console.WriteLine($"Entry Version: {entry.SystemProperties.Version}");
        //                    Console.WriteLine($"Entry Published Version: {entry.SystemProperties.PublishedVersion}");
        //                    Console.WriteLine($"Entry Published At: {entry.SystemProperties.PublishedAt}");
        //                    Console.WriteLine($"Entry Updated At: {entry.SystemProperties.UpdatedAt}");

        //                    var publichEntry = await managementClient.PublishEntry(id, (int)updatedVersion);
        //                }
        //            }
        //            else
        //            {
        //                continue;
        //            }

        //        }
        //    }

        //    return Ok();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Import(IFormFile file, ImportModel import)
        //{
        //    var managementClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);

        //    // Use EPPlus to read the Excel file
        //    var data = _excelImportService.ImportFromExcel(file);

        //    foreach (var sheet in data)
        //    {
        //        var name = sheet.Key.Split(",");
        //        var contentType = name[0];
        //        var locale = name[1].Replace(" ", "");

        //        var dataTable = sheet.Value;

        //        // Extract header row
        //        var headerRow = dataTable.Rows[0];
        //        var headers = headerRow.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

        //        // For each row in the Excel file, update the corresponding content in Contentful
        //        foreach (DataRow row in dataTable.Rows)
        //        {
        //            var id = row[0].ToString(); // Assuming the ID is in the first column
        //            var newName = row[1].ToString(); // Assuming the new name is in the second column


        //            // Get the existing entry with the current version
        //            var entry = await managementClient.GetEntry(id);

        //            if (entry == null)
        //            {
        //                Console.WriteLine($"Entry with ID {id} not found.");
        //                continue;
        //            }

        //            // Check if the field exists and has a value for the given locale
        //            if (!entry.Fields.ContainsKey("name") || !entry.Fields["name"].ContainsKey(locale) || entry.Fields["name"][locale] == null)
        //            {
        //                Console.WriteLine($"Name field is missing or null for entry with ID {id}.");
        //                continue;
        //            }

        //            var currentName = entry.Fields["name"][locale].ToString();

        //            // If the name has changed, update and publish the entry
        //            if (newName != currentName)
        //            {
        //                var regularEntryDto = new RegularEntryDto { Name = newName };
        //                var version = entry.SystemProperties.Version;

        //                try
        //                {
        //                    //// Unpublish the entry if it's already published
        //                    //if (entry.SystemProperties.Version == entry.SystemProperties.PublishedVersion)
        //                    //{
        //                    //    await managementClient.UnpublishEntry(id, (int)version);
        //                    //}

        //                    // Update the entry
        //                    var updatedEntry = await managementClient.UpdateEntryForLocale(regularEntryDto, id, locale);

        //                    // Publish the updated entry
        //                    var publishedEntry = await managementClient.PublishEntry(id, (int)updatedEntry.SystemProperties.Version);

        //                    Console.WriteLine($"Entry with ID {id} updated and published successfully.");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Error updating or publishing entry with ID {id}: {ex.Message}");
        //                }
        //            }
        //        }
        //    }

        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, ImportModel import)
        {
            var managementClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);

            // Use EPPlus to read the Excel file
            var data = _excelImportService.ImportFromExcel(file);

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

                    if (contentType == "productListingPage")
                    {
                        var fullExport = MapRowToFullExport(row, headers);

                        var fullEntryDto = MapFullExportToDto(fullExport);

                        // Get the existing entry with the current version
                        var entryFull = await managementClient.GetEntry(id);

                        if (entryFull == null)
                        {
                            Console.WriteLine($"Entry with ID {id} not found.");
                            continue;
                        }

                        //var regularEntryDto = MapEntryToDto(entry,locale); 

                        // Iterate over each column in the row (excluding the first one)
                        for (int columnIndex = 1; columnIndex < headers.Count; columnIndex++)
                        {
                            var header = headers[columnIndex];
                            var newValue = row[columnIndex];

                            var lowerCaseHeader = char.ToLowerInvariant(header[0]) + header.Substring(1);

                            object currentValue = null;

                            // Check if the field exists and has a value for the given locale
                            if (entryFull.Fields.ContainsKey(lowerCaseHeader) && entryFull.Fields[lowerCaseHeader].ContainsKey(locale) && entryFull.Fields[lowerCaseHeader][locale] != null)
                            {
                                var property = typeof(FullEntryDto).GetProperty(header);
                                if (property != null)
                                {
                                    property.SetValue(fullEntryDto, currentValue);
                                }
                                currentValue = entryFull.Fields[lowerCaseHeader][locale].ToString();
                            }
                            if (header == "CreatedAt" && entryFull.SystemProperties.CreatedAt.HasValue)
                            {
                                currentValue = entryFull.SystemProperties.CreatedAt.Value.Date;
                            }
                            if (header == "Tags")
                            {
                                currentValue = entryFull.Metadata.Tags.Select(tag => tag.Sys.Id).ToList();
                            }
                            if (header == "Filter")
                            {
                                fullEntryDto.Filter._rawFilterData = currentValue as string;

                            }

                            // If currentValue is still null, skip to the next header
                            if (currentValue == null)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        var regularExport = MapRowToRegularExport(row, headers);

                        var regularEntryDto = MapRegularExportToDto(regularExport);
                    }

                   

                    /////___________________________________________________________//


                    // Get the existing entry with the current version
                    var entry = await managementClient.GetEntry(id);

                    if (entry == null)
                    {
                        Console.WriteLine($"Entry with ID {id} not found.");
                        continue;
                    }

                    //var regularEntryDto = MapEntryToDto(entry,locale); 

                    // Iterate over each column in the row (excluding the first one)
                    for (int columnIndex = 1; columnIndex < headers.Count; columnIndex++)
                    {
                        var header = headers[columnIndex];
                        var newValue = row[columnIndex]; 

                        var lowerCaseHeader = header.ToLower();
                        
                        object currentValue = null;

                        // Check if the field exists and has a value for the given locale
                        if (entry.Fields.ContainsKey(lowerCaseHeader) && entry.Fields[lowerCaseHeader].ContainsKey(locale) && entry.Fields[lowerCaseHeader][locale] != null)
                        {
                            var property = typeof(FullEntryDto).GetProperty(header);
                            if (property != null)
                            {
                                //property.SetValue(fullEntryDto, currentValue);
                            }
                            currentValue = entry.Fields[lowerCaseHeader][locale].ToString();
                        }
                        if (header == "CreatedAt" && entry.SystemProperties.CreatedAt.HasValue)
                        {
                            currentValue = entry.SystemProperties.CreatedAt.Value.Date;
                        }
                        if (header == "Tags")
                        {
                            currentValue = entry.Metadata.Tags.Select(tag => tag.Sys.Id).ToList();
                        }
                        
                        // If currentValue is still null, skip to the next header
                        if (currentValue == null)
                        {
                            continue;
                        }
                        
                        //om det inte finns något värde så skapa en ny entry
                       
                        // Try to parse as boolean
                        if (bool.TryParse(currentValue.ToString(), out bool boolValue))
                        {
                            currentValue = boolValue;
                            if (bool.TryParse(newValue.ToString(), out bool newBoolValue))
                            {
                                newValue = newBoolValue;
                            }
                        }
                        // Try to parse as integer
                        else if (int.TryParse(currentValue.ToString(), out int intValue))
                        {
                            currentValue = intValue;
                            if (int.TryParse(newValue.ToString(), out int newIntValue))
                            {
                                newValue = newIntValue;
                            }
                        }
                        // Try to parse as DateTime
                        else if (DateTime.TryParse(currentValue.ToString(), out DateTime dateValue))
                        {
                            currentValue = dateValue;
                            if (DateTime.TryParse(newValue.ToString(), out DateTime newDateValue))
                            {
                                newValue = newDateValue;
                            }
                        }
                        

                        // If the value has changed, update the corresponding property in the RegularEntryDto object
                        if (newValue.ToString() != currentValue.ToString())
                        {
                            var property = typeof(RegularExport).GetProperty(header);
                            
                            if (property != null)
                            {


                               // property.SetValue(regularExport, newValue);
                            }

                            //entry version
                            var version = entry.SystemProperties.Version;

                            //try
                            //{
                            //   // Update the entry
                            //    var updatedEntry = await managementClient.UpdateEntryForLocale(regularExport, id, locale);

                            //    // Publish the updated entry
                            //    var publishedEntry = await managementClient.PublishEntry(id, (int)updatedEntry.SystemProperties.Version);

                            //    Console.WriteLine($"Entry with ID {id} updated and published successfully.");
                            //}
                            //catch (Exception ex)
                            //{
                            //    Console.WriteLine($"Error updating or publishing entry with ID {id}: {ex.Message}");
                            //}
                        }
                    }

                  
                }
            }

            return Ok();
        }

        private RegularEntryDto MapEntryToDto(Entry<dynamic> entry,string locale)
        {
            var dto = new RegularEntryDto()
            {
                Sys = entry.SystemProperties,
                InternalName = entry.Fields.ContainsKey("internalName") ? entry.Fields["internalName"][locale].ToString() : null,
                Name = entry.Fields.ContainsKey("name") ? entry.Fields["name"][locale].ToString() : null,
                Slug = entry.Fields.ContainsKey("slug") ? entry.Fields["slug"][locale].ToString() : null,
                //Urls = entry.Fields.ContainsKey("urls") ? JsonConvert.DeserializeObject<List<List<string>>>(entry.Fields["urls"][locale].ToString()) : null,
                Metadata = entry.Fields.ContainsKey("$metadata") ? JsonConvert.DeserializeObject<ContentfulMetadata>(entry.Fields["$metadata"][locale].ToString()) : null
            };

            if (entry.Fields.ContainsKey("urls"))
            {
                var urls = JsonConvert.DeserializeObject<List<List<string>>>(entry.Fields["urls"][locale].ToString());
                if (urls.Count > 0 && urls[urls.Count - 1].Count > 0)
                {
                    dto.Urls = new List<List<string>> { new List<string> { urls[urls.Count - 1][urls[urls.Count - 1].Count - 1] } };
                }
            }

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
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        // Try to parse the date in the format "yyyy-MM-dd"
                        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                        {
                            property.SetValue(regularExport, dateValue);
                        }
                        else
                        {
                            throw new FormatException($"Could not parse date: {value}");
                        }
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
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        // Try to parse the date in the format "yyyy-MM-dd"
                        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                        {
                            property.SetValue(fullExport, dateValue);
                        }
                        else
                        {
                            throw new FormatException($"Could not parse date: {value}");
                        }
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
            
            regularEntryDto.InternalName = regularExport.InternalName;
            regularEntryDto.Name = regularExport.Name;
            regularEntryDto.Slug = regularExport.Slug;


            if (!string.IsNullOrEmpty(regularExport.Urls))
            {
                regularEntryDto.Urls = new List<List<string>> { regularExport.Urls.Split(',').ToList() };
            }

            // Assuming Tags in RegularExport is a comma-separated string
            if (!string.IsNullOrEmpty(regularExport.Tags))
            {
                regularEntryDto.Metadata = new ContentfulMetadata
                {
                    Tags = regularExport.Tags.Split(',').Select(tag => new Reference { Sys = new ReferenceProperties { Id = tag } }).ToList()
                };
            }

            regularExport.CreatedAt = regularExport.CreatedAt;

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
            //fullEntryDto.AdditionalContentDescription = new Document { Content = fullExport.AdditionalContentDescription };
            fullEntryDto.Active = new List<string> { fullExport.Active.ToString() };
            fullEntryDto.H1Title = fullExport.H1Title;
            fullEntryDto.Slug = fullExport.Slug;
            fullEntryDto.Urls = new List<List<string>> { new List<string> { fullExport.Urls } };
            fullEntryDto.SeoInfo = new SeoInfo { Title = fullExport.SeoTitle, Description = fullExport.SeoDescription };

            return fullEntryDto;
        }



    }
}
