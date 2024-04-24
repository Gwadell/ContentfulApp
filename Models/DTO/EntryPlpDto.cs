using Contentful.Core.Models;

namespace ContentfulApp.Models.DTO
{
    public class EntryPlpDto
    {
        //public SystemProperties Sys { get; set; }

        public string Id { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public string ShortDescription { get; set; }
        public AdditionalContentDescription AdditionalContentDescription { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? Slug { get; set; }
        public string[]? Active { get; set; }
        public Filter Filter { get; set; }
        public string[]? Facets { get; set; }
        public string? Url { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class Filter
    {
    }

    public class AdditionalContentDescription
    {
    }
}
