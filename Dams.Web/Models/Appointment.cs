using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int SlotId { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public AppointmentSlot Slot { get; set; } = null!;
}
