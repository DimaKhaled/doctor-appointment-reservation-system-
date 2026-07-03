namespace Dams.Web.ViewModels.Patient;

public class PatientAppointmentsPageViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ActiveTab { get; set; } = string.Empty;

    public List<PatientAppointmentListItemViewModel> Appointments { get; set; } = [];
}
