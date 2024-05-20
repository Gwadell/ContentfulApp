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

                    // Map the row to a RegularExport object
                    var regularExport = MapRowToRegularExport(row, headers);

                    var regularEntryDto = MapRegularExportToDto(regularExport);

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
                        var newValue = row[columnIndex].ToString();

                        var lowerCaseHeader = header.ToLower();
                        
                        object currentValue = null;

                        // Check if the field exists and has a value for the given locale
                        if (entry.Fields.ContainsKey(lowerCaseHeader) && entry.Fields[lowerCaseHeader].ContainsKey(locale) && entry.Fields[lowerCaseHeader][locale] != null)
                        {
                            currentValue = entry.Fields[lowerCaseHeader][locale].ToString();
                        }

                        var property = typeof(RegularEntryDto).GetProperty(header, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                        // If the property exists and its value is not null, get the current value
                        if (property != null && property.GetValue(regularEntryDto) != null)
                        {
                             currentValue = entry.Fields[lowerCaseHeader][locale].ToString();
                        }

                        //om det inte finns något värde så skapa en ny entry


                        // If currentValue is still null, skip to the next header
                        if (currentValue == null)
                        {
                            continue;
                        }

                        // ta bort locale?? om  det inte finns på internalname?? 

                        // Try to parse as boolean
                        if (bool.TryParse(currentValue.ToString(), out bool boolValue))
                        {
                            currentValue = boolValue;
                        }
                        // Try to parse as integer
                        else if (int.TryParse(currentValue.ToString(), out int intValue))
                        {
                            currentValue = intValue;
                        }
                        // Try to parse as DateTime
                        else if (DateTime.TryParse(currentValue.ToString(), out DateTime dateValue))
                        {
                            currentValue = dateValue;
                        }
                        // If none of the above, keep as string
                        else
                        {
                            currentValue = currentValue.ToString();
                        }

                        // If the value has changed, update the corresponding property in the RegularEntryDto object
                        if (newValue != currentValue)
                        {
                             //property = typeof(RegularEntryDto).GetProperty(header, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (property != null)
                            {
                                property.SetValue(regularEntryDto, newValue);
                            }

                            // Update and publish the entry
                            var version = entry.SystemProperties.Version;

                            try
                            {
                                //// Unpublish the entry if it's already published
                                //if (entry.SystemProperties.Version == entry.SystemProperties.PublishedVersion)
                                //{
                                //    await managementClient.UnpublishEntry(id, (int)version);
                                //}

                                // Update the entry
                                var updatedEntry = await managementClient.UpdateEntryForLocale(regularEntryDto, id, locale);

                                // Publish the updated entry
                                var publishedEntry = await managementClient.PublishEntry(id, (int)updatedEntry.SystemProperties.Version);

                                Console.WriteLine($"Entry with ID {id} updated and published successfully.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error updating or publishing entry with ID {id}: {ex.Message}");
                            }
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
                    if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                    {
                        property.SetValue(regularExport, intValue);
                    }
                    else if (property.PropertyType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                    {
                        property.SetValue(regularExport, boolValue);
                    }
                    else if (property.PropertyType == typeof(DateTime) && DateTime.TryParse(value, out DateTime dateValue))
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

        private RegularEntryDto MapRegularExportToDto(RegularExport regularExport)
        {
            var regularEntryDto = new RegularEntryDto();

            regularEntryDto.InternalName = regularExport.InternalName;
            regularEntryDto.Name = regularExport.Name;
            regularEntryDto.Slug = regularExport.Slug;

            // Assuming Urls in RegularExport is a comma-separated string
            regularEntryDto.Urls = new List<List<string>> { regularExport.Urls.Split(',').ToList() };

            // Assuming Tags in RegularExport is a comma-separated string
            if (!string.IsNullOrEmpty(regularExport.Tags))
            {
                regularEntryDto.Metadata = new ContentfulMetadata
                {
                    Tags = regularExport.Tags.Split(',').Select(tag => new Reference { Sys = new ReferenceProperties { Id = tag } }).ToList()
                };
            }

            return regularEntryDto;
        }




    }
}
