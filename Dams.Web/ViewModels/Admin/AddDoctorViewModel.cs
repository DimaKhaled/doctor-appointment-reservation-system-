using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Admin
{
    public class AddDoctorViewModel
    {
        [Required, StringLength(100, MinimumLength = 3), Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;


        [Required, Phone, StringLength(20), Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required]
        public string Gender { get; set; } = string.Empty;

        
        [Required]
        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        
        [Required]
        [Display(Name = "Clinic")]
        public int ClinicId { get; set; }


        [Required]
        [StringLength(1000)]
        public string Qualifications { get; set; } = string.Empty;


        [Required]
        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int ExperienceYears { get; set; }


        [Required]
        [StringLength(1000)]
        public string Biography { get; set; } = string.Empty;


        [Required, DataType(DataType.Password), Display(Name = "Initial Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[_#$!*%]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, and one special character: _ # $ ! * %.")]
        public string InitialPassword { get; set; } = string.Empty;


        [Required, DataType(DataType.Password), Compare(nameof(InitialPassword)), Display(Name = "Confirm Initial Password")]
        public string ConfirmInitialPassword { get; set; } = string.Empty;


        // Dropdown data
        public IEnumerable<SelectListItem> Specializations { get; set; } = [];

        public IEnumerable<SelectListItem> Clinics { get; set; } = [];
    }
}
