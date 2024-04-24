using Contentful.Core.Models;

namespace ContentfulApp.Models.DTO
{
    public class EntryBrandDto
    {
        public SystemProperties Sys { get; set; }

        public string InternalName { get; set; }
        public string? Name { get; set; }
        public string ShortDescription { get; set; }
    }
}
