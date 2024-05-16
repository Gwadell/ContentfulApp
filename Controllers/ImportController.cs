using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using ContentfulApp.Models;
using ContentfulApp.Models.DTO;
using ContentfulApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;

namespace ContentfulApp.Controllers
{
    public class ImportController : Controller
    {
        private readonly IContentfulManagementClient _client;
        private readonly IExcelImportService _excelImportService; 
        private readonly IContentfulService _contentfulService;

        public IActionResult Index()
        {

            return View();
        }

        public ImportController(IContentfulManagementClient client, IContentfulService contentfulService, IExcelImportService excelImportServide)
        {
            _client = client;
            _contentfulService = contentfulService;
            _excelImportService = excelImportServide;
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file, ImportModel import)
        {
            var managmentClient = await _contentfulService.GetManagementClient(import.AccessToken, import.SpaceId, import.Environment);
            var client =await _contentfulService.GetClientAsync(import.AccessToken, import.SpaceId, import.Environment);
            // Use EPPlus to read the Excel file
            var data = _excelImportService.ImportFromExcel(file);

            foreach (var sheet in data)
            {
                var name = sheet.Key.Split(","); 
                var contentType = name[0];
                var locale = name[1].Replace(" ", "");   

                var dataTable = sheet.Value;

                //For each row in the Excel file, update the corresponding content in Contentful
                foreach (DataRow row in dataTable.Rows)
                {
                    string id = row[0]?.ToString(); // Assuming the ID is in the first column
                    string newName = row[1]?.ToString(); // Assuming the new name is in the second column

                    // Get the existing entry
                    var entryDynamic = await managmentClient.GetEntry(id);
                    var version = entryDynamic.SystemProperties.Version;


                    //var entryasJson = JsonConvert.SerializeObject(entryDynamic);

                    // Update the Name field based on the data from the Excel file
                    var property = typeof(RegularEntryDto).GetProperty("Name");
                    if (entryDynamic.Fields.ContainsKey("name"))
                    {
                        // Convert newName to the correct type
                        var convertedName = Convert.ChangeType(newName, property.PropertyType);

                        // Convert the object to a JToken
                        var jToken = JToken.FromObject(convertedName);

                        // Set the property value for the specific locale
                        entryDynamic.Fields["name"][locale] = jToken;
                    }
                    else
                    {
                        // Handle the case where the field does not exist
                        // For example, you could log a warning message
                        Console.WriteLine($"Warning: Field 'name' does not exist in entry {id}");
                    }


                    entryDynamic.SystemProperties.Version = version;

                    // Update the entry in Contentful
                    var updatedEntry = await managmentClient.CreateOrUpdateEntry(entryDynamic);
                    //var updatedEntry = await managmentClient.UpdateEntryForLocale(entryDynamic, locale);
                }

            }

            return Ok();
        }
    }
}
