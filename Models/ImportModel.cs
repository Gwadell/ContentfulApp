namespace ContentfulApp.Models
{
    public class ImportModel
    {
        public string Environment { get; set; } 
        public string SpaceId { get; set; } 
        public string AccessToken { get; set; }
        public bool PublishChanges { get; set; } = true; 
    }
}
