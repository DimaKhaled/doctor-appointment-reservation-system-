namespace Dams.Web.ViewModels.Patient;

public class PatientDashboardViewModel
{
    public string PatientName { get; set; } = string.Empty;

    public string? ProfilePicturePath { get; set; }

    public int TotalActiveDoctors { get; set; }

    // Placeholders — will be wired up when Person 2 finishes the Appointments feature
    public int UpcomingAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public List<DoctorListItemViewModel> FeaturedDoctors { get; set; } = [];
}