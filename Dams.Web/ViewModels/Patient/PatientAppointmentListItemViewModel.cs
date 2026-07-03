namespace Dams.Web.ViewModels.Patient;

public class PatientAppointmentListItemViewModel
{
    public int AppointmentId { get; set; }

    public int DoctorId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string SpecializationName { get; set; } = string.Empty;

    public string ClinicName { get; set; } = string.Empty;

    public string ClinicCity { get; set; } = string.Empty;

    public DateTime SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool CanCancel { get; set; }

    public bool CanReview { get; set; }

    public bool HasReview { get; set; }
}
