namespace Humanitarian.Application.DTOs
{
    public class CreateCrisisDto
    {
        public string Title { get; set; }
        public string CrisisType { get; set; } // උදා: Flood, Landslide
        public string Severity { get; set; }  // උදා: High, Critical
        public string OperationalArea { get; set; }
    }
}