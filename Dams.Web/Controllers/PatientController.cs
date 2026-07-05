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

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);

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
            UpcomingAppointments = await context.Appointments.CountAsync(a =>
                a.PatientId == patient.PatientId &&
                (a.Status == AppStatuses.Pending || a.Status == AppStatuses.Confirmed) &&
                a.Slot.SlotDate >= today),
            CompletedAppointments = await context.Appointments.CountAsync(a =>
                a.PatientId == patient.PatientId &&
                a.Status == AppStatuses.Completed),
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

    // ---------- FR-15: Book Appointment ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(int doctorId, int slotId)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var strategy = context.Database.CreateExecutionStrategy();
        var bookingOutcome = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            var slot = await context.AppointmentSlots
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.SlotId == slotId && s.DoctorId == doctorId);

            if (slot is null)
            {
                return "missing";
            }

            var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);
            if (slot.SlotDate < today)
            {
                return "past";
            }

            if (slot.Doctor.Status != AppStatuses.Active)
            {
                return "inactive-doctor";
            }

            var slotAlreadyReserved = slot.IsBooked || await context.Appointments.AnyAsync(a =>
                a.SlotId == slot.SlotId &&
                (a.Status == AppStatuses.Pending || a.Status == AppStatuses.Confirmed));
            if (slotAlreadyReserved)
            {
                return "booked";
            }

            slot.IsBooked = true;
            context.Appointments.Add(new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = doctorId,
                SlotId = slotId,
                Status = AppStatuses.Pending
            });

            try
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return "success";
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return "booked";
            }
        });

        if (bookingOutcome == "missing")
        {
            TempData["ErrorMessage"] = "The selected appointment slot could not be found.";
            return RedirectToAction(nameof(DoctorProfile), new { id = doctorId });
        }

        if (bookingOutcome == "past")
        {
            TempData["ErrorMessage"] = "You can only book upcoming appointment slots.";
            return RedirectToAction(nameof(DoctorProfile), new { id = doctorId });
        }

        if (bookingOutcome == "inactive-doctor")
        {
            TempData["ErrorMessage"] = "This doctor is not available for booking right now.";
            return RedirectToAction(nameof(DoctorProfile), new { id = doctorId });
        }

        if (bookingOutcome == "booked")
        {
            TempData["ErrorMessage"] = "This slot is no longer available. Please try another appointment time.";
            return RedirectToAction(nameof(DoctorProfile), new { id = doctorId });
        }

        TempData["SuccessMessage"] = "Appointment booked successfully. Waiting for doctor confirmation.";
        return RedirectToAction(nameof(UpcomingAppointments));
    }

    // ---------- FR-16 / FR-17 / FR-18 / FR-19 / FR-20: Manage Appointments ----------

    public async Task<IActionResult> UpcomingAppointments()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);
        var appointments = await context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patient.PatientId &&
                        (a.Status == AppStatuses.Pending || a.Status == AppStatuses.Confirmed) &&
                        a.Slot.SlotDate >= today)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Doctor).ThenInclude(d => d.Clinic)
            .Include(a => a.Slot)
            .OrderBy(a => a.Slot.SlotDate).ThenBy(a => a.Slot.StartTime)
            .Select(a => new PatientAppointmentListItemViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                SpecializationName = a.Doctor.Specialization.Name,
                ClinicName = a.Doctor.Clinic.ClinicName,
                ClinicCity = a.Doctor.Clinic.City,
                SlotDate = a.Slot.SlotDate,
                StartTime = a.Slot.StartTime,
                EndTime = a.Slot.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                CanCancel = a.Status == AppStatuses.Pending || a.Status == AppStatuses.Confirmed
            })
            .ToListAsync();

        return View(new PatientAppointmentsPageViewModel
        {
            Title = "Upcoming Appointments",
            Description = "Track your pending and confirmed bookings, and cancel them when needed.",
            ActiveTab = "Upcoming",
            Appointments = appointments
        });
    }

    public async Task<IActionResult> AppointmentHistory()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);
        var reviewedDoctorIds = await context.Reviews
            .AsNoTracking()
            .Where(r => r.PatientId == patient.PatientId)
            .Select(r => r.DoctorId)
            .ToListAsync();

        var reviewedDoctorIdSet = reviewedDoctorIds.ToHashSet();

        var appointmentEntities = await context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == patient.PatientId &&
                        (a.Slot.SlotDate < today ||
                         a.Status == AppStatuses.Completed ||
                         a.Status == AppStatuses.Cancelled ||
                         a.Status == AppStatuses.Rejected))
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Doctor).ThenInclude(d => d.Clinic)
            .Include(a => a.Slot)
            .OrderByDescending(a => a.Slot.SlotDate).ThenByDescending(a => a.Slot.StartTime)
            .ToListAsync();

        var appointments = appointmentEntities
            .Select(a => new PatientAppointmentListItemViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor.User.FullName,
                SpecializationName = a.Doctor.Specialization.Name,
                ClinicName = a.Doctor.Clinic.ClinicName,
                ClinicCity = a.Doctor.Clinic.City,
                SlotDate = a.Slot.SlotDate,
                StartTime = a.Slot.StartTime,
                EndTime = a.Slot.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                HasReview = reviewedDoctorIdSet.Contains(a.DoctorId),
                CanReview = a.Status == AppStatuses.Completed && !reviewedDoctorIdSet.Contains(a.DoctorId)
            })
            .ToList();

        return View(new PatientAppointmentsPageViewModel
        {
            Title = "Appointment History",
            Description = "Review your previous, cancelled, and completed appointments.",
            ActiveTab = "History",
            Appointments = appointments
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var appointment = await context.Appointments
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == id && a.PatientId == patient.PatientId);

        if (appointment is null)
        {
            return NotFound();
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);
        if (appointment.Slot.SlotDate < today)
        {
            TempData["ErrorMessage"] = "Past appointments cannot be cancelled.";
            return RedirectToAction(nameof(UpcomingAppointments));
        }

        if (appointment.Status != AppStatuses.Pending && appointment.Status != AppStatuses.Confirmed)
        {
            TempData["ErrorMessage"] = $"Appointments with status '{appointment.Status}' cannot be cancelled.";
            return RedirectToAction(nameof(UpcomingAppointments));
        }

        appointment.Status = AppStatuses.Cancelled;
        appointment.Slot.IsBooked = false;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(UpcomingAppointments));
    }

    // ---------- FR-21: Submit Doctor Review ----------

    public async Task<IActionResult> CreateReview(int doctorId)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var doctor = await GetEligibleDoctorForReviewAsync(patient.PatientId, doctorId);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "You can only review a doctor after completing an appointment with them.";
            return RedirectToAction(nameof(AppointmentHistory));
        }

        return View(new CreateReviewViewModel
        {
            DoctorId = doctor.DoctorId,
            DoctorName = doctor.User.FullName,
            SpecializationName = doctor.Specialization.Name,
            ClinicName = doctor.Clinic.ClinicName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReview(CreateReviewViewModel model)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var doctor = await GetEligibleDoctorForReviewAsync(patient.PatientId, model.DoctorId);
        if (doctor is null)
        {
            TempData["ErrorMessage"] = "You are not eligible to review this doctor.";
            return RedirectToAction(nameof(AppointmentHistory));
        }

        if (!ModelState.IsValid)
        {
            model.DoctorName = doctor.User.FullName;
            model.SpecializationName = doctor.Specialization.Name;
            model.ClinicName = doctor.Clinic.ClinicName;
            return View(model);
        }

        context.Reviews.Add(new Review
        {
            PatientId = patient.PatientId,
            DoctorId = doctor.DoctorId,
            Rating = model.Rating,
            ReviewText = model.ReviewText.Trim()
        });

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "You have already submitted a review for this doctor.";
            return RedirectToAction(nameof(AppointmentHistory));
        }

        TempData["SuccessMessage"] = "Thank you. Your review has been submitted.";
        return RedirectToAction(nameof(DoctorProfile), new { id = doctor.DoctorId });
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

    private async Task<Doctor?> GetEligibleDoctorForReviewAsync(int patientId, int doctorId)
    {
        var hasCompletedAppointment = await context.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.DoctorId == doctorId &&
            a.Status == AppStatuses.Completed);

        if (!hasCompletedAppointment)
        {
            return null;
        }

        var alreadyReviewed = await context.Reviews.AnyAsync(r =>
            r.PatientId == patientId &&
            r.DoctorId == doctorId);

        if (alreadyReviewed)
        {
            return null;
        }

        return await context.Doctors
            .AsNoTracking()
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

    }
}
