namespace Dams.Web.ViewModels.Doctor;

public class AppointmentListItemViewModel
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DoctorDashboardViewModel
{
    public string DoctorName { get; set; } = string.Empty;
    public int PendingCount { get; set; }
    public int ConfirmedUpcomingCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalAppointments { get; set; }
    public int ReviewCount { get; set; }
    public double AverageRating { get; set; }
    public List<AppointmentListItemViewModel> TodayAppointments { get; set; } = [];
    public List<AppointmentListItemViewModel> PendingAppointments { get; set; } = [];
}

public class PatientDetailsViewModel
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? ProfilePicturePath { get; set; }
    public List<AppointmentListItemViewModel> AppointmentsWithThisDoctor { get; set; } = [];
}
