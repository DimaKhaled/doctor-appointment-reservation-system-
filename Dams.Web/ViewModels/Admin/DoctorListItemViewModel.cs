namespace Dams.Web.ViewModels.Admin
{
    public class DoctorListItemViewModel
    {
        public int DoctorId { get; set; }

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        public string Specialization { get; set; } = "";

        public string Clinic { get; set; } = "";

        public int ExperienceYears { get; set; }

        public string Status { get; set; } = "";
    }
}
