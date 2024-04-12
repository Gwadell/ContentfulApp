using Contentful.Core;
using ContentfulApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            // Retrieve content types from Contentful
            var contentTypes = await _client.GetContentTypes();

            // Extract content type names and store them in a list
            var contentTypeNames = new List<string>();
            foreach (var contentType in contentTypes)
            {
                contentTypeNames.Add(contentType.SystemProperties.Id); // or Name if you prefer
            }

            // Pass the list of content type names to the view
            return View(contentTypeNames);
        }

        public IActionResult Privacy()
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
