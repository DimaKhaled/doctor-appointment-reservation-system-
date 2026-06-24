using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Clinic
{
    public int ClinicId { get; set; }

    [Required, StringLength(150)]
    public string ClinicName { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public ICollection<Doctor> Doctors { get; set; } = [];
}
