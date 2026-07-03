namespace Dams.Web.ViewModels.Patient;

public class PatientDashboardViewModel
{
    public string PatientName { get; set; } = string.Empty;

    public string? ProfilePicturePath { get; set; }

    public int TotalActiveDoctors { get; set; }

    public int UpcomingAppointments { get; set; }

    public int CompletedAppointments { get; set; }

    public List<DoctorListItemViewModel> FeaturedDoctors { get; set; } = [];
}
