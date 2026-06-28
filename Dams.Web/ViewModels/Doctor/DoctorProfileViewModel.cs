using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Dams.Web.ViewModels.Doctor;

public class DoctorProfileViewModel
{
    public int DoctorId { get; set; }

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Specialization")]
    public string SpecializationName { get; set; } = string.Empty;

    [Display(Name = "Clinic")]
    public string ClinicName { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [StringLength(1000), Display(Name = "Qualifications")]
    public string? Qualifications { get; set; }

    [Range(0, 80, ErrorMessage = "Experience must be between 0 and 80 years."), Display(Name = "Years of Experience")]
    public int ExperienceYears { get; set; }

    [StringLength(1000), Display(Name = "Biography")]
    public string? Biography { get; set; }

    [Display(Name = "Current Profile Picture")]
    public string? ProfilePicturePath { get; set; }

    [Display(Name = "Upload New Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }
}
