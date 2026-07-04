using System.Security.Claims;
using Dams.Web.Data;
using Dams.Web.Models;
using Dams.Web.ViewModels.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class PatientController(DamsDbContext context, IWebHostEnvironment environment) : Controller
{
    private const long MaxProfilePictureBytes = 5 * 1024 * 1024; // 5 MB, per FR-7
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];

    public async Task<IActionResult> Index()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
            return RedirectToAction("AccessDenied", "Account");

        var featuredDoctors = await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .Where(d => d.Status == AppStatuses.Active)
            .OrderBy(d => d.DoctorId)
            .Take(3)
            .Select(d => new DoctorListItemViewModel
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                SpecializationName = d.Specialization.Name,
                Gender = d.User.Gender,
                ExperienceYears = d.ExperienceYears,
                ClinicName = d.Clinic.ClinicName,
                City = d.Clinic.City,
                ProfilePicturePath = d.ProfilePicturePath,
                AverageRating = d.Reviews.Count > 0 ? Math.Round(d.Reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = d.Reviews.Count
            })
            .ToListAsync();

        var model = new PatientDashboardViewModel
        {
            PatientName = patient.User.FullName,
            ProfilePicturePath = patient.ProfilePicturePath,
            TotalActiveDoctors = await context.Doctors.CountAsync(d => d.Status == AppStatuses.Active),
            UpcomingAppointments = 0,
            CompletedAppointments = 0,
            FeaturedDoctors = featuredDoctors
        };

        return View(model);
    }

    // ---------- FR-5 / FR-6 / FR-7: View / Edit Patient Profile, Upload Picture ----------

    public async Task<IActionResult> Profile()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return View(BuildProfileViewModel(patient));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(PatientProfileViewModel model)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (model.ProfilePicture is not null)
        {
            var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "Only JPG or PNG images are allowed.");
            }
            else if (model.ProfilePicture.Length > MaxProfilePictureBytes)
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "Image must be 5 MB or smaller.");
            }
        }

        // Email, Gender, and Date of Birth are display-only (FR-6): the form never binds
        // them via asp-for, so the model's values for these fields are always whatever
        // ASP.NET defaults to on POST. They are reloaded from the database below and
        // never written back, regardless of what the client submits.
        if (!ModelState.IsValid)
        {
            var refreshed = BuildProfileViewModel(patient);
            refreshed.FullName = model.FullName;
            refreshed.PhoneNumber = model.PhoneNumber;
            refreshed.BloodType = model.BloodType;
            refreshed.Allergies = model.Allergies;
            refreshed.ChronicDiseases = model.ChronicDiseases;
            return View(refreshed);
        }

        patient.User.FullName = model.FullName.Trim();
        patient.User.PhoneNumber = model.PhoneNumber.Trim();
        patient.BloodType = string.IsNullOrWhiteSpace(model.BloodType) ? null : model.BloodType.Trim();
        patient.Allergies = string.IsNullOrWhiteSpace(model.Allergies) ? null : model.Allergies.Trim();
        patient.ChronicDiseases = string.IsNullOrWhiteSpace(model.ChronicDiseases) ? null : model.ChronicDiseases.Trim();

        if (model.ProfilePicture is not null)
        {
            patient.ProfilePicturePath = await SaveProfilePictureAsync(patient.PatientId, model.ProfilePicture);
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    // ---------- FR-8 to FR-13: Browse / Search / Filter / Sort Doctors ----------

    public async Task<IActionResult> Doctors(DoctorSearchViewModel filters)
    {
        var query = context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .Where(d => d.Status == AppStatuses.Active);

        // FR-9: search by doctor name or specialization.
        if (!string.IsNullOrWhiteSpace(filters.Keyword))
        {
            var keyword = filters.Keyword.Trim();
            query = query.Where(d =>
                d.User.FullName.Contains(keyword) ||
                d.Specialization.Name.Contains(keyword));
        }

        // FR-10: filter by specialization.
        if (filters.SpecializationId.HasValue)
        {
            query = query.Where(d => d.SpecializationId == filters.SpecializationId.Value);
        }

        // FR-11: filter by clinic location (city and/or specific clinic).
        if (!string.IsNullOrWhiteSpace(filters.City))
        {
            query = query.Where(d => d.Clinic.City == filters.City);
        }

        if (filters.ClinicId.HasValue)
        {
            query = query.Where(d => d.ClinicId == filters.ClinicId.Value);
        }

        // FR-12: filter by gender.
        if (!string.IsNullOrWhiteSpace(filters.Gender))
        {
            query = query.Where(d => d.User.Gender == filters.Gender);
        }

        // FR-13: sort results.
        query = filters.SortBy switch
        {
            DoctorSearchViewModel.SortOptions.NameDesc => query.OrderByDescending(d => d.User.FullName),
            DoctorSearchViewModel.SortOptions.ExperienceDesc => query.OrderByDescending(d => d.ExperienceYears),
            DoctorSearchViewModel.SortOptions.ExperienceAsc => query.OrderBy(d => d.ExperienceYears),
            _ => query.OrderBy(d => d.User.FullName)
        };

        var doctors = await query
            .Select(d => new DoctorListItemViewModel
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                SpecializationName = d.Specialization.Name,
                Gender = d.User.Gender,
                ExperienceYears = d.ExperienceYears,
                ClinicName = d.Clinic.ClinicName,
                City = d.Clinic.City,
                ProfilePicturePath = d.ProfilePicturePath,
                AverageRating = d.Reviews.Count > 0 ? Math.Round(d.Reviews.Average(r => r.Rating), 1) : 0,
                ReviewCount = d.Reviews.Count
            })
            .ToListAsync();

        filters.Doctors = doctors;
        filters.SpecializationOptions = await context.Specializations
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecializationOptionViewModel { SpecializationId = s.SpecializationId, Name = s.Name })
            .ToListAsync();
        filters.ClinicOptions = await context.Clinics
            .AsNoTracking()
            .OrderBy(c => c.ClinicName)
            .Select(c => new ClinicOptionViewModel { ClinicId = c.ClinicId, ClinicName = c.ClinicName })
            .ToListAsync();
        filters.CityOptions = await context.Clinics
            .AsNoTracking()
            .Select(c => c.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return View(filters);
    }

    // ---------- FR-14: View Doctor Profile (Patient View) ----------

    public async Task<IActionResult> DoctorProfile(int id)
    {
        var doctor = await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .FirstOrDefaultAsync(d => d.DoctorId == id && d.Status == AppStatuses.Active);

        if (doctor is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);

        var availableSlots = await context.AppointmentSlots
            .AsNoTracking()
            .Where(s => s.DoctorId == id && !s.IsBooked && s.SlotDate >= today)
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .Select(s => new AvailableSlotViewModel
            {
                SlotId = s.SlotId,
                SlotDate = s.SlotDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            })
            .ToListAsync();

        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.DoctorId == id)
            .Include(r => r.Patient).ThenInclude(p => p.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DoctorReviewItemViewModel
            {
                ReviewerName = r.Patient.User.FullName,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var model = new DoctorDetailsViewModel
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.User.FullName,
            ProfilePicturePath = doctor.ProfilePicturePath,
            Gender = doctor.User.Gender,
            SpecializationName = doctor.Specialization.Name,
            Qualifications = doctor.Qualifications,
            ExperienceYears = doctor.ExperienceYears,
            Biography = doctor.Biography,
            ClinicName = doctor.Clinic.ClinicName,
            ClinicAddress = doctor.Clinic.Address,
            ClinicCity = doctor.Clinic.City,
            AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
            ReviewCount = reviews.Count,
            AvailableSlots = availableSlots,
            Reviews = reviews
        };

        return View(model);
    }

    // ---------- Helpers ----------

    private async Task<Patient?> GetCurrentPatientAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return null;
        }

        return await context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == parsedUserId);
    }

    private static PatientProfileViewModel BuildProfileViewModel(Patient patient)
    {
        return new PatientProfileViewModel
        {
            PatientId = patient.PatientId,
            FullName = patient.User.FullName,
            Email = patient.User.Email,
            PhoneNumber = patient.User.PhoneNumber,
            Gender = patient.User.Gender,
            DateOfBirth = patient.DateOfBirth,
            BloodType = patient.BloodType,
            Allergies = patient.Allergies,
            ChronicDiseases = patient.ChronicDiseases,
            ProfilePicturePath = patient.ProfilePicturePath
        };
    }

    private async Task<string> SaveProfilePictureAsync(int patientId, IFormFile file)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "images", "patients");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"patient-{patientId}{extension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/patients/{fileName}";
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(int slotId, int doctorId)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
            return RedirectToAction("AccessDenied", "Account");

        // Check slot exists and is not booked
        var slot = await context.AppointmentSlots
            .FirstOrDefaultAsync(s => s.SlotId == slotId && !s.IsBooked);

        if (slot is null)
        {
            TempData["ErrorMessage"] = "This slot is no longer available.";
            return RedirectToAction("DoctorProfile", new { id = doctorId });
        }

        // Prevent double booking
        var alreadyBooked = await context.Appointments
            .AnyAsync(a => a.PatientId == patient.PatientId && a.SlotId == slotId);

        if (alreadyBooked)
        {
            TempData["ErrorMessage"] = "You have already booked this slot.";
            return RedirectToAction("DoctorProfile", new { id = doctorId });
        }

        // Create appointment
        var appointment = new Appointment
        {
            PatientId = patient.PatientId,
            DoctorId = doctorId,
            SlotId = slotId,
            Status = AppStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };

        slot.IsBooked = true;

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Appointment booked successfully. Waiting for doctor confirmation.";
        return RedirectToAction("Index");
    }
}
