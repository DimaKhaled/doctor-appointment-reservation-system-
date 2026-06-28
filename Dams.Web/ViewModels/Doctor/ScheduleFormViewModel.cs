using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Doctor;

public class ScheduleFormViewModel
{
    public int ScheduleId { get; set; }

    [Required, Display(Name = "Day of Week")]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required, DataType(DataType.Time), Display(Name = "Start Time")]
    public TimeOnly StartTime { get; set; } = new(9, 0);

    [Required, DataType(DataType.Time), Display(Name = "End Time")]
    public TimeOnly EndTime { get; set; } = new(17, 0);

    [Required, Display(Name = "Slot Duration (minutes)")]
    public int SlotDurationMinutes { get; set; } = 30;

    public static readonly string[] DaysOfWeek =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    public static readonly int[] AllowedDurations = [15, 30, 45, 60];
}

public class ScheduleListItemViewModel
{
    public int ScheduleId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; }
    public int UpcomingSlotCount { get; set; }
    public int BookedUpcomingSlotCount { get; set; }
}
