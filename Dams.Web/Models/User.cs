using System.ComponentModel.DataAnnotations;

namespace Dams.Web.Models;

public class User
{
    public int UserId { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient? Patient { get; set; }

    public Doctor? Doctor { get; set; }

    public Admin? Admin { get; set; }
}
