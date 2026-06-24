namespace Dams.Web.Models;

public class AppointmentSlot
{
    public int SlotId { get; set; }

    public int ScheduleId { get; set; }

    public int DoctorId { get; set; }

    public DateTime SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool IsBooked { get; set; }

    public Schedule Schedule { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;

    public Appointment? Appointment { get; set; }
}
