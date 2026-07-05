namespace Dams.Web.ViewModels.Admin
{
    public class PatientListItemViewModel
    {
        public int PatientId { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        public string Gender { get; set; } = "";

        public DateTime DateOfBirth { get; set; }

        public string BloodType { get; set; } = "";
    }
}
