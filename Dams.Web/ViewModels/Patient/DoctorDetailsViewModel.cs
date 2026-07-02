namespace Dams.Web.ViewModels.Patient;

public class DoctorDetailsViewModel
{
    public int DoctorId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? ProfilePicturePath { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string SpecializationName { get; set; } = string.Empty;

    public string? Qualifications { get; set; }

    public int ExperienceYears { get; set; }

    public string? Biography { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string ClinicAddress { get; set; } = string.Empty;

    public string ClinicCity { get; set; } = string.Empty;

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public List<AvailableSlotViewModel> AvailableSlots { get; set; } = [];

    public List<DoctorReviewItemViewModel> Reviews { get; set; } = [];
}

public class AvailableSlotViewModel
{
    public int SlotId { get; set; }

    public DateTime SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}

public class DoctorReviewItemViewModel
{
    public string ReviewerName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string ReviewText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
