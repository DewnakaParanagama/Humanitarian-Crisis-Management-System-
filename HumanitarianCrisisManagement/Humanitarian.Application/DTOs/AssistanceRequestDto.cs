namespace Humanitarian.Application.DTOs
{
    public class AssistanceRequestDto
    {
        public string CitizenId { get; set; }
        public string CrisisId { get; set; }
        public string Category { get; set; } 
        public string UrgencyLevel { get; set; } 
        public string Description { get; set; }
        public string LocationCoordinates { get; set; }
    }
}