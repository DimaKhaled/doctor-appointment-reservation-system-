using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Account;

public class RegisterViewModel
{
    [Required, StringLength(100, MinimumLength = 3), Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20), Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required, DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[_#$!*%]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, and one special character: _ # $ ! * %.")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
