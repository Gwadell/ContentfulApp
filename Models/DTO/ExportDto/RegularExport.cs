namespace ContentfulApp.Models.DTO.ExportDto
{
    public class RegularExport
    {
        public string Id { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }

        public string Urls { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string Tags { get; set; }

        //archived
    }
}
