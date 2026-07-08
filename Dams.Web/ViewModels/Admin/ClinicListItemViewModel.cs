namespace Dams.Web.ViewModels.Admin;

public class ClinicListItemViewModel
{
    public int ClinicId { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int DoctorsCount { get; set; }

    public bool CanDelete { get; set; }
}
