namespace Dams.Web.ViewModels.Admin;

public class AppointmentMonitoringViewModel
{
    public string? SearchTerm { get; set; }

    public IReadOnlyList<AppointmentListItemViewModel> Appointments { get; set; } = [];
}
