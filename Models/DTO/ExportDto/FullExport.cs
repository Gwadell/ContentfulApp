
namespace ContentfulApp.Models.DTO.ExportDto
{
    public class FullExport
    {
        public string Id { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public int CategoryRank { get; set; }
        public string ShortDescription { get; set; }
        public bool IsPrimaryCategory { get; set; }
        public string? Filter { get; set; }
        public bool CreateLinksOnProductPages { get; set; }
        public string SubPageData { get; set; } 
        public bool UseAsFacet { get; set; }
        public string? AdditionalContentDescription { get; set; } 
        public bool Active { get; set; }
        public string? H1Title { get; set; }
        public string?  Tags { get; set; } 
        public string? Facets { get; set; }
        public string Urls { get; set; }
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? Slug { get; set; }
        public bool Archived { get; set; }  //funkar ej på delivery api
        public DateTime? CreatedAt { get; set; }

    }
}
