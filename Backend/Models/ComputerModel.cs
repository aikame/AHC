using System.Text.Json;

namespace Backend.Models
{
    public class ComputerModel
    {
        public Guid? Id { get; set; }
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public JsonElement Domain { get; set; }
        public float TotalRAMGB { get; set; }
        public List<JsonElement> DiskSpace { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public int ComputerRole { get; set; }
        public bool Status { get; set; } = true;

    }
}
