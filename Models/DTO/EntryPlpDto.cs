using Contentful.Core.Models;
using System.Text.Json.Serialization;
using System.Xml.Schema;

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
        public Filter? Filter { get; set; }
        //public SubPageData? SubPageData { get; set; }
        public  AdditionalContentDescription? AdditionalContentDescription  { get; set; }

        public List<string> Active { get; set; }

        public bool CreateLinksOnProductPages { get; set; }
        public bool UseAsFacet { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? Facets { get; set; }

        public string? H1Title { get; set; }

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
                foreach (string locale in Active)
                {
                    if (locale.Equals(Sys.Locale, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public string? GetFacetsAsString()
        {
            return Facets != null ? string.Join(",", Facets) : null;
        }

        public string? GetTagsAsString()
        {
            return Tags != null ? string.Join(",", Tags) : null;
        }

        public string GetAdditionalContentDescriptionAsString()
        {
            if (AdditionalContentDescription != null)
            {
                return AdditionalContentDescription.Content.Select(c => c.Value).ToString();
            }
            return null;
        }



        // funkar bara på mangement api. 
        public bool IsArchived()
        {
            return Sys?.ArchivedAt != null;
        }



    }

    //public class SubPageData
    //{
    //    public string? Routes { get; set; }
    //}

    public class SeoInfo
    {
        public string? Title { get; set; }

        public string? Description { get; set; }
    }


    public class Filter
    {
        public string? _rawFilterData { get; set; }
    }

    public class AdditionalContentDescription
    {
        //public string? Data { get; set; }
        public List<Content>? Content { get; set; }
    }

    public class Content
    {
        public string? Value { get; set; }
    }
}
