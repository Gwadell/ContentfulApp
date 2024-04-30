using Contentful.Core;
using Contentful.Core.Models;
using Contentful.Core.Search;
using ContentfulApp.Models;
using ContentfulApp.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.AccessControl;

namespace ContentfulApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContentfulClient _client;
        private readonly IContentfulManagementClient _managementClient;
        

        public HomeController(ILogger<HomeController> logger, IContentfulClient client, IContentfulManagementClient contentfulManagementClient)
        {
            _logger = logger;
            _client = client;
            _managementClient = contentfulManagementClient;
        }

  

        public async Task<IActionResult> ExportContentTypes(List<string> contentTypesList, List<string> localesList)
        {
            var exportedDataList = new List<ContentTypeExportData>();

            var entries = await _managementClient.GetEntriesCollection<dynamic>();

            var entriesAsJson = JsonConvert.SerializeObject(entries);

            foreach (var contenttype in contentTypesList)
            {
                var fetchedItemsList = new List<dynamic>(); 
                int totalItemsCOunt = 0;
                int fetchedItemsCount = 0;

                var queryBuilder = QueryBuilder<dynamic>.New.ContentTypeIs(contenttype).Limit(10); 

                do
                {
                    //var entries = await _client.GetEntries<dynamic>(queryBuilder);
                    totalItemsCOunt = entries.Total;
                    fetchedItemsCount += entries.Items.Count();

                    foreach (var entry in entries.Items)
                    {
                        fetchedItemsList.Add(entry);
                    }
                } while (fetchedItemsCount < totalItemsCOunt);

                var exportedData = new ContentTypeExportData
                {
                    ContentType = contenttype,
                    Items = fetchedItemsList
                };

                exportedDataList.Add(exportedData);
            }

            return View(exportedDataList);
        }

        public async Task<IActionResult> Index()
        {
            //var entries = await _managementClient.GetEntriesCollection<dynamic>();

            //var entriesAsJson = JsonConvert.SerializeObject(entries);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Export() 
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
