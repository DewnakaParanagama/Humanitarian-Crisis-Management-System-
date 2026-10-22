using System.ComponentModel.DataAnnotations;

namespace Humanitarian.Application.DTOs
{
    public class RegisterCitizenDto
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "NIC is required")]
        [StringLength(12, MinimumLength = 10, ErrorMessage = "NIC must be between 10 and 12 characters")]
        public string Nic { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [Phone(ErrorMessage = "Invalid Contact Number format")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string Location { get; set; }
    }
}