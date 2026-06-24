using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Schedule
{
    public int ScheduleId { get; set; }

    public int DoctorId { get; set; }

    [Required, StringLength(10)]
    public string DayOfWeek { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDurationMinutes { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = [];
}
