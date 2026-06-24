using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class Review
{
    public int ReviewId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(1000)]
    public string ReviewText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;

    public Doctor Doctor { get; set; } = null!;
}
