#nullable disable

using System.ComponentModel.DataAnnotations;

namespace MovieProject.ViewModels
{
    public class ContactMe
    {
        [Required]
        [StringLength(80, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(80, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 5)]
        public string Email { get; set; }

        [Phone]
        [Display(Name= "Phone (optional)")]
        
        public string PhoneNumber { get; set; }

        public string Subject { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be between {2} and {1} characters.", MinimumLength = 2)]
        public string Message { get; set; } 

    }
}
