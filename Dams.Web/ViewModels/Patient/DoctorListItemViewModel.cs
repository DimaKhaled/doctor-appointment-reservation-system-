namespace Dams.Web.ViewModels.Patient;

public class DoctorListItemViewModel
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string SpecializationName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? ProfilePicturePath { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}
