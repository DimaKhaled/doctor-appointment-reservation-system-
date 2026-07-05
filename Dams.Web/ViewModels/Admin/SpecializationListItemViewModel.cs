namespace Dams.Web.ViewModels.Admin
{
    public class SpecializationListItemViewModel
    {
        public int SpecializationId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DoctorsCount { get; set; }

        public bool CanDelete { get; set; } 
    }
}
