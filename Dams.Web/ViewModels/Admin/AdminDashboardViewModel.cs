namespace Dams.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int MonthlyAppointments { get; set; }
        public int MonthlyCompletedAppointments { get; set; }
        public int MonthlyCancelledAppointments { get; set; }
        public int MonthlyNewPatients { get; set; }
    }
}
