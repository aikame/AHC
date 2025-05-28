using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ADDC.Models.Data
{
    public class ComputerModel
    {
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public Guid DomainId { get; set; }
        public JObject Domain { get; set; }
        public float TotalRAMGB { get; set; }
        public List<JsonElement> DiskSpace { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public int ComputerRole { get; set; }
    }
}
