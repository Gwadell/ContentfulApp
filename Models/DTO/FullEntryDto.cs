﻿using Contentful.Core.Models;

namespace ContentfulApp.Models.DTO
{

    class FullEntryDto
    {
        public SystemProperties Sys { get; set; } // id and createdAt
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public bool IsPrimaryCategory { get; set; }
        public int CategoryRank { get; set; }
        public string ShortDescription { get; set; }
        public Filter? Filter { get; set; }
        //public SubPageData? SubPageData { get; set; }
        public Document? AdditionalContentDescription { get; set; }
        public List<string> Active { get; set; }
        public bool CreateLinksOnProductPages { get; set; }
        public bool UseAsFacet { get; set; }
        public SeoInfo? SeoInfo { get; set; }
        //public List<string> Tags { get; set; }
        public List<string>? Facets { get; set; }
        public string? H1Title { get; set; }
        public string Slug { get; set; }
        public List<List<string>> Urls { get; set; }


        /// <summary>
        /// Retrieves the last URL from the Urls list.
        /// </summary>
        /// <returns>The last URL as a string, or null if the Urls list is empty.</returns>
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

        /// <summary>
        /// Checks if the current locale is active.
        /// </summary>
        /// <returns>True if the locale is active, false otherwise.</returns>
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

        /// <summary>
        /// Converts the Facets list to a comma-separated string.
        /// </summary>
        /// <returns>The Facets as a string, or null if the Facets list is empty.</returns>
        public string? GetFacetsAsString()
        {
            return Facets != null ? string.Join(",", Facets) : null;
        }

        // funkar bara på mangement api. 
        public bool IsArchived()
        {
            return Sys?.ArchivedAt != null;
        }

        /// <summary>
        /// Converts the AdditionalContentDescription to HTML.
        /// </summary>
        /// <returns>The AdditionalContentDescription as HTML string, or null if it is null.</returns>
        public string ConvertAdditionalContentDescriptionToHtml()
        {
            if (AdditionalContentDescription != null)
            {
                var html = new HtmlRenderer().ToHtml(AdditionalContentDescription);
                return html.ToString();
            }
            return null;
        }

    }

    public class SeoInfo
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    //public class SubPageData
    //{
    //    public string? Routes { get; set; }
    //}

    public class Filter
    {
        public string? _rawFilterData { get; set; }
    }

}
