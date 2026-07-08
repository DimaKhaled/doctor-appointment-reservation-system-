namespace Dams.Web.ViewModels.Admin;

public class AppointmentListItemViewModel
{
    public int AppointmentId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string ClinicName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
