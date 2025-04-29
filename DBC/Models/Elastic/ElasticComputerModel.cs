using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DBC.Models.Elastic
{
    public class ElasticComputerModel
    {
        public Guid Id { get; set; }
        public DateTime Updated { get; set; } = DateTime.UtcNow;
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public string DomainName { get; set; }
        public float TotalRAMGB { get; set; }

        public List<JsonElement> DiskSpace { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public int ComputerRole { get; set; }
        public bool Status { get; set; } = true;

    }
}
