using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Dams.Web.ViewModels.Patient;

public class PatientProfileViewModel
{
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Full name is required."), StringLength(100, MinimumLength = 3), Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required."), Phone(ErrorMessage = "Please enter a valid phone number."), Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Gender")]
    public string Gender { get; set; } = string.Empty;

    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [StringLength(5), Display(Name = "Blood Type")]
    public string? BloodType { get; set; }

    [StringLength(500), Display(Name = "Allergies")]
    public string? Allergies { get; set; }

    [StringLength(500), Display(Name = "Chronic Diseases")]
    public string? ChronicDiseases { get; set; }

    [Display(Name = "Current Profile Picture")]
    public string? ProfilePicturePath { get; set; }

    [Display(Name = "Upload New Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }
}
