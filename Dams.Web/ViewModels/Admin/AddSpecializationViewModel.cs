using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Admin
{
    public class AddSpecializationViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Specialization Name")]
        public string Name { get; set; } = string.Empty;
    }
}
