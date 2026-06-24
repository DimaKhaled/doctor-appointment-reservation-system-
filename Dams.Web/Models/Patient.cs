using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Patient
{
    public int PatientId { get; set; }

    public int UserId { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [StringLength(5)]
    public string? BloodType { get; set; }

    [StringLength(500)]
    public string? Allergies { get; set; }

    [StringLength(500)]
    public string? ChronicDiseases { get; set; }

    [StringLength(300)]
    public string? ProfilePicturePath { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<Review> Reviews { get; set; } = [];
}
