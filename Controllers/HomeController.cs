using Contentful.Core;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.AccessControl;

namespace ContentfulApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContentfulClient _client;
        

        public HomeController(ILogger<HomeController> logger, IContentfulClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<IActionResult> Index()
        {
            var contentTypes = await _client.GetContentTypes();

            var contentTypeNames = new List<string>();
            foreach (var contentType in contentTypes)
            {
                contentTypeNames.Add(contentType.SystemProperties.Id); 
            }

            return View(contentTypeNames);
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
