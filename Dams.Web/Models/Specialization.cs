using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Specialization
{
    public int SpecializationId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Doctor> Doctors { get; set; } = [];
}
