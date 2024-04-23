using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ContentfulApp.Models.DTO
{
    public class  ContentTypeExportData
    {
        public string ContentType { get; set; }
        public List<dynamic> Items { get; set; }
    }
}
