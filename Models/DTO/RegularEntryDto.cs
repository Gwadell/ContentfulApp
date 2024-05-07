using Contentful.Core.Models;
using Newtonsoft.Json;

namespace ContentfulApp.Models.DTO
{
    public class RegularEntryDto
    {
        public SystemProperties Sys { get; set; }
        public string InternalName { get; set; }
        public string? Name { get; set; }
        public string Slug { get; set; }
        public List<List<string>> Urls { get; set; }
        [JsonProperty("$metadata")]
        public ContentfulMetadata? Metadata { get; set; }


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

        public string GetTagsIdsAsString()
        {
            return Metadata?.Tags != null ? string.Join(",", Metadata.Tags.Select(tag => tag.Sys.Id)) : null;
        }
    }
}
