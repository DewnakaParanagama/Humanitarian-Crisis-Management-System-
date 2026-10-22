using System.ComponentModel.DataAnnotations;

namespace Humanitarian.Application.DTOs
{
    public class CreateCrisisDto
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Crisis Type is required")]
        public string CrisisType { get; set; } 

        [Required(ErrorMessage = "Severity is required")]
        public string Severity { get; set; }  

        [Required(ErrorMessage = "Operational Area is required")]
        public string OperationalArea { get; set; }
    }
}