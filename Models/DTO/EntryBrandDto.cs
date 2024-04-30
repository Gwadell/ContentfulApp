using Contentful.Core.Models;

namespace ContentfulApp.Models.DTO
{
    public class EntryBrandDto
    {
        public SystemProperties Sys { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public string Slug { get; set; }
        public List<List<string>> Urls { get; set; }


        public string GetLastUrl()
        {
            if (Urls != null && Urls.Count > 0)
            {
                List<string> lastList = Urls[Urls.Count - 1];
                if (lastList != null && lastList.Count > 0)
                {
                    return lastList[lastList.Count - 1];
                }
            }
            return null;
        }
    }
}
