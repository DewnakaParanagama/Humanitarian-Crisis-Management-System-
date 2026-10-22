using System.ComponentModel.DataAnnotations;

namespace Humanitarian.Application.DTOs
{
    public class AssistanceRequestDto
    {
        [Required(ErrorMessage = "Citizen ID is required")]
        public string CitizenId { get; set; }

        [Required(ErrorMessage = "Crisis ID is required")]
        public string CrisisId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } 

        [Required(ErrorMessage = "Urgency Level is required")]
        public string UrgencyLevel { get; set; } 

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Location Coordinates are required")]
        public string LocationCoordinates { get; set; }
    }
}