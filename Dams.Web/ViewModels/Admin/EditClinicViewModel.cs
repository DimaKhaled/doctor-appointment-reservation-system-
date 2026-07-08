using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Admin;

public class EditClinicViewModel
{
    public int ClinicId { get; set; }

    [Required, StringLength(150), Display(Name = "Clinic Name")]
    public string ClinicName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, Phone, StringLength(20), Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;
}
