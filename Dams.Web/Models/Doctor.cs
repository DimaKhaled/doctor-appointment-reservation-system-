using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Doctor
{
    public int DoctorId { get; set; }

    public int UserId { get; set; }

    public int SpecializationId { get; set; }

    public int ClinicId { get; set; }

    [StringLength(1000)]
    public string? Qualifications { get; set; }

    [Range(0, int.MaxValue)]
    public int ExperienceYears { get; set; }

    [StringLength(1000)]
    public string? Biography { get; set; }

    [StringLength(300)]
    public string? ProfilePicturePath { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Active";

    public User User { get; set; } = null!;

    public Specialization Specialization { get; set; } = null!;

    public Clinic Clinic { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = [];

    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<Review> Reviews { get; set; } = [];
}
