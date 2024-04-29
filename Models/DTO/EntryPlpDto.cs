using Contentful.Core.Models;
using System.Text.Json.Serialization;

namespace ContentfulApp.Models.DTO
{
   
 class EntryPlpDto
    {
        public SystemProperties Sys { get; set; } // id and createdAt
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public bool IsPrimaryCategory { get; set; }
        public int CategoryRank { get; set; }
        public string ShortDescription { get; set; }
        public FilterObj? FilterObj { get; set; }
        //public string SubPageData { get; set; }
        //public string AdditionalContentDescription { get; set; }

        public List<string> Active { get; set; }

        public bool CreateLinksOnProductPages { get; set; }
        public bool UseAsFacet { get; set; }
        //public string Tags { get; set; }
        //public string Facets { get; set; }

        public SeoInfo? SeoInfo { get; set; }
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

        public bool IsActiveLocale()
        {
            if (Active != null && Sys != null && !string.IsNullOrEmpty(Sys.Locale))
            {
                foreach (string activeString in Active)
                {
                    if (activeString.Equals(Sys.Locale, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class SeoInfo
    {
        public string? Title { get; set; }

        public string? Description { get; set; }
    }

    public class Urls
    {

    }

    public class FilterObj
    {
        [JsonPropertyName("_rawFilterData")]
        public string? Filter { get; set; }
    }

    public class AdditionalContentDescription
    {
        public Validations?  Validations { get; set; }
    }

    public class Validations
    {
        public string? Message { get; set; }
    }
}
