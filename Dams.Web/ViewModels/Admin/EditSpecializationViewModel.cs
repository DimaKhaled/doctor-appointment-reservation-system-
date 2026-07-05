using System.ComponentModel.DataAnnotations;

namespace Dams.Web.ViewModels.Admin
{
    public class EditSpecializationViewModel
    {
        public int SpecializationId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Specialization Name")]
        public string Name { get; set; } = string.Empty;
    }
}
