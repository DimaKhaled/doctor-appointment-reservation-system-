using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Patient;

public class CreateReviewViewModel
{
    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string SpecializationName { get; set; } = string.Empty;

    public string ClinicName { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 1000 characters.")]
    public string ReviewText { get; set; } = string.Empty;
}
