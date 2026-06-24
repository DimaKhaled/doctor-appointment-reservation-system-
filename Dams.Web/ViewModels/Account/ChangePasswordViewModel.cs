using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Account;

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "New Password")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[_#$!*%]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, and one special character: _ # $ ! * %.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Confirm New Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
