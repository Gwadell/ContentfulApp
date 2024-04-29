using Contentful.Core.Models;
using ContentfulApp.Models.DTO;
using System.Text.Json.Serialization;

namespace ContentfulApp.Models.DTO.ExportDto
{
    public class EntryPlpDtoExport
    {
        public string Id { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public bool IsPrimaryCategory { get; set; }
        public int CategoryRank { get; set; }
        public string ShortDescription { get; set; }
        public string? Filter { get; set; }

        //public string SubPageData { get; set; }
        //public string AdditionalContentDescription { get; set; }
        public bool Active { get; set; }
        public bool CreateLinksOnProductPages { get; set; }

        public bool UseAsFacet { get; set; }
        //public string  Tags { get; set; }

        //public string Facets { get; set; }

        public string Url { get; set; }
        public string? Title { get; set; }
        
        public string? Description { get; set; }
        public string? Slug { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
