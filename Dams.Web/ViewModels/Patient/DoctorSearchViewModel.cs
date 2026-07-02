namespace Dams.Web.ViewModels.Patient;

public class DoctorSearchViewModel
{
    /// <summary>FR-9: free-text search across doctor name and specialization.</summary>
    public string? Keyword { get; set; }

    /// <summary>FR-10: filter by specialization id.</summary>
    public int? SpecializationId { get; set; }

    /// <summary>FR-11: filter by clinic city.</summary>
    public string? City { get; set; }

    /// <summary>FR-11: filter by clinic id.</summary>
    public int? ClinicId { get; set; }

    /// <summary>FR-12: filter by doctor gender (Male / Female).</summary>
    public string? Gender { get; set; }

    /// <summary>FR-13: sort order applied to results.</summary>
    public string? SortBy { get; set; }

    public List<DoctorListItemViewModel> Doctors { get; set; } = [];

    // ---------- Filter dropdown sources ----------

    public List<SpecializationOptionViewModel> SpecializationOptions { get; set; } = [];

    public List<ClinicOptionViewModel> ClinicOptions { get; set; } = [];

    public List<string> CityOptions { get; set; } = [];

    public static class SortOptions
    {
        public const string NameAsc = "name_asc";
        public const string NameDesc = "name_desc";
        public const string ExperienceDesc = "experience_desc";
        public const string ExperienceAsc = "experience_asc";

        public static readonly string[] All = [NameAsc, NameDesc, ExperienceDesc, ExperienceAsc];
    }
}

public class SpecializationOptionViewModel
{
    public int SpecializationId { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class ClinicOptionViewModel
{
    public int ClinicId { get; set; }

    public string ClinicName { get; set; } = string.Empty;
}
