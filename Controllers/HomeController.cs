using Contentful.Core;
using Contentful.Core.Search;
using ContentfulApp.Models;
using ContentfulApp.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

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
