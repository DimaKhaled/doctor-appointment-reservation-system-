using System.Security.Claims;
using Dams.Web.Data;
using Dams.Web.Models;
using Dams.Web.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Controllers;

[Authorize(Roles = AppRoles.Doctor)]
public class DoctorController(DamsDbContext context, IWebHostEnvironment environment) : Controller
{
    private const int SlotGenerationWeeksAhead = 4;
    private const long MaxProfilePictureBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    // ---------- FR-32 / FR-33: Doctor Dashboard & Appointment Statistics ----------

    public async Task<IActionResult> Index()
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);

        var appointments = await context.Appointments
            .Where(a => a.DoctorId == doctor.DoctorId)
            .Include(a => a.Slot)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .ToListAsync();

        var reviews = await context.Reviews
            .Where(r => r.DoctorId == doctor.DoctorId)
            .ToListAsync();

        var model = new DoctorDashboardViewModel
        {
            DoctorName = doctor.User.FullName,
            TotalAppointments = appointments.Count,
            PendingCount = appointments.Count(a => a.Status == AppStatuses.Pending),
            ConfirmedUpcomingCount = appointments.Count(a =>
                a.Status == AppStatuses.Confirmed && a.Slot.SlotDate >= today),
            CompletedCount = appointments.Count(a => a.Status == AppStatuses.Completed),
            RejectedCount = appointments.Count(a => a.Status == AppStatuses.Rejected),
            CancelledCount = appointments.Count(a => a.Status == AppStatuses.Cancelled),
            ReviewCount = reviews.Count,
            AverageRating = reviews.Count > 0 ? Math.Round(reviews.Average(r => r.Rating), 1) : 0,
            TodayAppointments = appointments
                .Where(a => a.Slot.SlotDate == today &&
                            (a.Status == AppStatuses.Confirmed || a.Status == AppStatuses.Pending))
                .OrderBy(a => a.Slot.StartTime)
                .Select(ToListItem)
                .ToList(),
            PendingAppointments = appointments
                .Where(a => a.Status == AppStatuses.Pending)
                .OrderBy(a => a.Slot.SlotDate).ThenBy(a => a.Slot.StartTime)
                .Select(ToListItem)
                .ToList()
        };

        return View(model);
    }

    // ---------- FR-22 / FR-23 / FR-24: View / Edit Doctor Profile, Upload Picture ----------

    public async Task<IActionResult> Profile()
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return View(BuildProfileViewModel(doctor));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(DoctorProfileViewModel model)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (model.ProfilePicture is not null)
        {
            var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();

            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "Only JPG, PNG, or WEBP images are allowed.");
            }
            else if (model.ProfilePicture.Length > MaxProfilePictureBytes)
            {
                ModelState.AddModelError(nameof(model.ProfilePicture), "Image must be 2 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            var refreshed = BuildProfileViewModel(doctor);
            refreshed.Qualifications = model.Qualifications;
            refreshed.ExperienceYears = model.ExperienceYears;
            refreshed.Biography = model.Biography;
            return View(refreshed);
        }

        doctor.Qualifications = model.Qualifications?.Trim();
        doctor.ExperienceYears = model.ExperienceYears;
        doctor.Biography = model.Biography?.Trim();

        if (model.ProfilePicture is not null)
        {
            doctor.ProfilePicturePath = await SaveProfilePictureAsync(doctor.DoctorId, model.ProfilePicture);
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    // ---------- FR-25 / FR-26 / FR-27: Create / Update / Delete Schedule ----------

    public async Task<IActionResult> Schedules()
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);

        var schedules = await context.Schedules
            .Where(s => s.DoctorId == doctor.DoctorId)
            .Include(s => s.AppointmentSlots)
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
            .Select(s => new ScheduleListItemViewModel
            {
                ScheduleId = s.ScheduleId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDurationMinutes = s.SlotDurationMinutes,
                UpcomingSlotCount = s.AppointmentSlots.Count(sl => sl.SlotDate >= today),
                BookedUpcomingSlotCount = s.AppointmentSlots.Count(sl => sl.SlotDate >= today && sl.IsBooked)
            })
            .ToListAsync();

        ViewBag.Days = ScheduleFormViewModel.DaysOfWeek;
        return View(schedules);
    }

    public IActionResult CreateSchedule()
    {
        return View(new ScheduleFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(ScheduleFormViewModel model)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        await ValidateScheduleFormAsync(model, doctor.DoctorId, excludeScheduleId: null);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var schedule = new Schedule
        {
            DoctorId = doctor.DoctorId,
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            SlotDurationMinutes = model.SlotDurationMinutes
        };

        context.Schedules.Add(schedule);
        await context.SaveChangesAsync();

        GenerateSlotsForSchedule(schedule);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Schedule created and appointment slots generated.";
        return RedirectToAction(nameof(Schedules));
    }

    public async Task<IActionResult> EditSchedule(int id)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var schedule = await context.Schedules
            .FirstOrDefaultAsync(s => s.ScheduleId == id && s.DoctorId == doctor.DoctorId);

        if (schedule is null)
        {
            return NotFound();
        }

        return View(new ScheduleFormViewModel
        {
            ScheduleId = schedule.ScheduleId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            SlotDurationMinutes = schedule.SlotDurationMinutes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchedule(int id, ScheduleFormViewModel model)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var schedule = await context.Schedules
            .Include(s => s.AppointmentSlots).ThenInclude(sl => sl.Appointment)
            .FirstOrDefaultAsync(s => s.ScheduleId == id && s.DoctorId == doctor.DoctorId);

        if (schedule is null)
        {
            return NotFound();
        }

        await ValidateScheduleFormAsync(model, doctor.DoctorId, excludeScheduleId: schedule.ScheduleId);

        var today = DateOnly.FromDateTime(DateTime.Today).ToDateTime(TimeOnly.MinValue);
        var hasFutureBookedSlots = schedule.AppointmentSlots.Any(sl =>
            sl.SlotDate >= today && sl.IsBooked &&
            sl.Appointment is { Status: AppStatuses.Pending or AppStatuses.Confirmed });

        if (hasFutureBookedSlots)
        {
            ModelState.AddModelError(string.Empty,
                "This schedule has upcoming booked appointments and cannot be changed. " +
                "Resolve those appointments first, or create a new schedule instead.");
        }

        if (!ModelState.IsValid)
        {
            model.ScheduleId = schedule.ScheduleId;
            return View(model);
        }

        schedule.DayOfWeek = model.DayOfWeek;
        schedule.StartTime = model.StartTime;
        schedule.EndTime = model.EndTime;
        schedule.SlotDurationMinutes = model.SlotDurationMinutes;

        // Remove future unbooked slots so they can be regenerated with the new times.
        var slotsToRemove = schedule.AppointmentSlots
            .Where(sl => sl.SlotDate >= today && !sl.IsBooked)
            .ToList();
        context.AppointmentSlots.RemoveRange(slotsToRemove);
        await context.SaveChangesAsync();

        GenerateSlotsForSchedule(schedule);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Schedule updated successfully.";
        return RedirectToAction(nameof(Schedules));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSchedule(int id)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var schedule = await context.Schedules
            .Include(s => s.AppointmentSlots).ThenInclude(sl => sl.Appointment)
            .FirstOrDefaultAsync(s => s.ScheduleId == id && s.DoctorId == doctor.DoctorId);

        if (schedule is null)
        {
            return NotFound();
        }

        var hasAnyAppointment = schedule.AppointmentSlots.Any(sl => sl.Appointment is not null);
        if (hasAnyAppointment)
        {
            TempData["ErrorMessage"] = "Cannot delete this schedule because it has appointments associated with it.";
            return RedirectToAction(nameof(Schedules));
        }

        context.Schedules.Remove(schedule);
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Schedule deleted successfully.";
        return RedirectToAction(nameof(Schedules));
    }

    // ---------- FR-28 to FR-31: View / Accept / Reject / Complete Appointments ----------

    public async Task<IActionResult> Appointments(string? status)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var query = context.Appointments
            .Where(a => a.DoctorId == doctor.DoctorId)
            .Include(a => a.Slot)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            query = query.Where(a => a.Status == status);
        }

        var appointments = await query
            .OrderByDescending(a => a.Slot.SlotDate).ThenByDescending(a => a.Slot.StartTime)
            .Select(a => new AppointmentListItemViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                SlotDate = a.Slot.SlotDate,
                StartTime = a.Slot.StartTime,
                EndTime = a.Slot.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        ViewBag.SelectedStatus = string.IsNullOrWhiteSpace(status) ? "All" : status;
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptAppointment(int id)
    {
        return await TransitionAppointmentAsync(id, fromStatus: AppStatuses.Pending, toStatus: AppStatuses.Confirmed,
            freeSlotOnTransition: false, successMessage: "Appointment accepted.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAppointment(int id)
    {
        return await TransitionAppointmentAsync(id, fromStatus: AppStatuses.Pending, toStatus: AppStatuses.Rejected,
            freeSlotOnTransition: true, successMessage: "Appointment rejected. The slot is now available again.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        return await TransitionAppointmentAsync(id, fromStatus: AppStatuses.Confirmed, toStatus: AppStatuses.Completed,
            freeSlotOnTransition: false, successMessage: "Appointment marked as completed.");
    }

    // ---------- FR-32: View Patient Profile (read-only, doctor's side) ----------

    public async Task<IActionResult> PatientProfile(int id)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        // Only patients who have at least one appointment with this doctor are visible.
        var hasRelationship = await context.Appointments
            .AnyAsync(a => a.DoctorId == doctor.DoctorId && a.PatientId == id);

        if (!hasRelationship)
        {
            return NotFound();
        }

        var patient = await context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == id);

        if (patient is null)
        {
            return NotFound();
        }

        var appointments = await context.Appointments
            .Where(a => a.DoctorId == doctor.DoctorId && a.PatientId == id)
            .Include(a => a.Slot)
            .OrderByDescending(a => a.Slot.SlotDate).ThenByDescending(a => a.Slot.StartTime)
            .Select(a => new AppointmentListItemViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                SlotDate = a.Slot.SlotDate,
                StartTime = a.Slot.StartTime,
                EndTime = a.Slot.EndTime,
                Status = a.Status,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var model = new PatientDetailsViewModel
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
            ProfilePicturePath = patient.ProfilePicturePath,
            AppointmentsWithThisDoctor = appointments
        };

        return View(model);
    }

    // ---------- Helpers ----------

    private async Task<Doctor?> GetCurrentDoctorAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            return null;
        }

        return await context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .FirstOrDefaultAsync(d => d.UserId == parsedUserId);
    }

    private static DoctorProfileViewModel BuildProfileViewModel(Doctor doctor)
    {
        return new DoctorProfileViewModel
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.User.FullName,
            Email = doctor.User.Email,
            PhoneNumber = doctor.User.PhoneNumber,
            SpecializationName = doctor.Specialization.Name,
            ClinicName = doctor.Clinic.ClinicName,
            Status = doctor.Status,
            Qualifications = doctor.Qualifications,
            ExperienceYears = doctor.ExperienceYears,
            Biography = doctor.Biography,
            ProfilePicturePath = doctor.ProfilePicturePath
        };
    }

    private async Task<string> SaveProfilePictureAsync(int doctorId, IFormFile file)
    {
        var uploadsFolder = Path.Combine(environment.WebRootPath, "images", "doctors");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"doctor-{doctorId}{extension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/doctors/{fileName}";
    }

    private async Task ValidateScheduleFormAsync(ScheduleFormViewModel model, int doctorId, int? excludeScheduleId)
    {
        if (!ScheduleFormViewModel.DaysOfWeek.Contains(model.DayOfWeek))
        {
            ModelState.AddModelError(nameof(model.DayOfWeek), "Please select a valid day of week.");
        }

        if (!ScheduleFormViewModel.AllowedDurations.Contains(model.SlotDurationMinutes))
        {
            ModelState.AddModelError(nameof(model.SlotDurationMinutes), "Slot duration must be 15, 30, 45, or 60 minutes.");
        }

        if (model.StartTime >= model.EndTime)
        {
            ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
        }

        if (!ModelState.IsValid)
        {
            return;
        }

        var existingSchedules = await context.Schedules
            .Where(s => s.DoctorId == doctorId && s.DayOfWeek == model.DayOfWeek &&
                        (excludeScheduleId == null || s.ScheduleId != excludeScheduleId))
            .ToListAsync();

        var overlaps = existingSchedules.Any(s =>
            model.StartTime < s.EndTime && s.StartTime < model.EndTime);

        if (overlaps)
        {
            ModelState.AddModelError(string.Empty,
                $"This overlaps with an existing schedule on {model.DayOfWeek}. Please choose a different time range.");
        }
    }

    private void GenerateSlotsForSchedule(Schedule schedule)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var targetDayOfWeek = Enum.Parse<DayOfWeek>(schedule.DayOfWeek);

        for (var offset = 0; offset <= SlotGenerationWeeksAhead * 7; offset++)
        {
            var date = today.AddDays(offset);
            if (date.DayOfWeek != targetDayOfWeek)
            {
                continue;
            }

            var slotStart = schedule.StartTime;
            while (slotStart.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
            {
                var slotEnd = slotStart.AddMinutes(schedule.SlotDurationMinutes);

                context.AppointmentSlots.Add(new AppointmentSlot
                {
                    ScheduleId = schedule.ScheduleId,
                    DoctorId = schedule.DoctorId,
                    SlotDate = date.ToDateTime(TimeOnly.MinValue),
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsBooked = false
                });

                slotStart = slotEnd;
            }
        }
    }

    private async Task<IActionResult> TransitionAppointmentAsync(
        int appointmentId, string fromStatus, string toStatus, bool freeSlotOnTransition, string successMessage)
    {
        var doctor = await GetCurrentDoctorAsync();
        if (doctor is null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var appointment = await context.Appointments
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

        if (appointment is null)
        {
            return NotFound();
        }

        if (appointment.Status != fromStatus)
        {
            TempData["ErrorMessage"] =
                $"This appointment is currently '{appointment.Status}' and cannot be transitioned from '{fromStatus}'.";
            return RedirectToAction(nameof(Appointments));
        }

        appointment.Status = toStatus;

        if (freeSlotOnTransition)
        {
            appointment.Slot.IsBooked = false;
        }

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = successMessage;
        return RedirectToAction(nameof(Appointments));
    }

    private static AppointmentListItemViewModel ToListItem(Appointment a)
    {
        return new AppointmentListItemViewModel
        {
            AppointmentId = a.AppointmentId,
            PatientId = a.PatientId,
            PatientName = a.Patient.User.FullName,
            SlotDate = a.Slot.SlotDate,
            StartTime = a.Slot.StartTime,
            EndTime = a.Slot.EndTime,
            Status = a.Status,
            CreatedAt = a.CreatedAt
        };
    }
}
