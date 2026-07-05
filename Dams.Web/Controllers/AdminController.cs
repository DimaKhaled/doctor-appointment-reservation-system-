using Dams.Web.Data;
using Dams.Web.Models;
using Dams.Web.Services;
using Dams.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class AdminController(DamsDbContext context, IPasswordService passwordService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var totalDoctors = await context.Doctors.CountAsync();
        var totalPatients = await context.Patients.CountAsync();
        var totalAppointments = await context.Appointments.CountAsync();
        var now = DateTime.UtcNow;
        var monthlyAppointments = await context.Appointments.Where(app => app.Slot.SlotDate.Month == now.Month && app.Slot.SlotDate.Year == now.Year).CountAsync();
        
        var monthlyCompletedAppointments = await context.Appointments.Where(app => app.Slot.SlotDate.Month == now.Month && app.Slot.SlotDate.Year == now.Year
                                                                                && app.Status == AppStatuses.Completed).CountAsync();
        var monthlyCancelledAppointments = await context.Appointments.Where(app => app.Slot.SlotDate.Month == now.Month && app.Slot.SlotDate.Year == now.Year
                                                                                && app.Status == AppStatuses.Cancelled).CountAsync();
        var monthlyNewPatients = await context.Patients.Where(p => p.User.CreatedAt.Month == now.Month && p.User.CreatedAt.Year == now.Year).CountAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalAppointments = totalAppointments,
            MonthlyAppointments = monthlyAppointments,
            MonthlyCompletedAppointments = monthlyCompletedAppointments,
            MonthlyCancelledAppointments = monthlyCancelledAppointments,
            MonthlyNewPatients = monthlyNewPatients
        };

        return View(viewModel);
    }


    [HttpGet]
    public async Task<IActionResult> AddDoctor()
    {
        var model = new AddDoctorViewModel();
        await PopulateDropDown(model);
        
        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDoctor(AddDoctorViewModel model)
    {
        // Validate Specialization exists
        if (!await context.Specializations.AnyAsync(s =>
            s.SpecializationId == model.SpecializationId))
        {
            ModelState.AddModelError(nameof(model.SpecializationId),
                "Selected specialization does not exist.");
        }

        // Validate Clinic exists
        if (!await context.Clinics.AnyAsync(c =>
            c.ClinicId == model.ClinicId))
        {
            ModelState.AddModelError(nameof(model.ClinicId),
                "Selected clinic does not exist.");
        }

        // Email uniqueness
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        if (await context.Users.AnyAsync(u => u.Email == normalizedEmail))
        {
            ModelState.AddModelError(nameof(model.Email),
                "Email already exists.");
        }

        // Phone uniqueness
        if (await context.Users.AnyAsync(u =>
            u.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber),
                "Phone number already exists.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropDown(model);
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = model.PhoneNumber.Trim(),
            Gender = model.Gender,
            Role = AppRoles.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            passwordService.HashPassword(user, model.InitialPassword);

        var doctor = new Doctor
        {
            User = user,
            SpecializationId = model.SpecializationId,
            ClinicId = model.ClinicId,
            Qualifications = model.Qualifications.Trim(),
            ExperienceYears = model.ExperienceYears,
            Biography = model.Biography.Trim(),
            Status = AppStatuses.Active
        };

        context.Doctors.Add(doctor);

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Doctor account created successfully.";

        return RedirectToAction(nameof(Index));
    }


    private async Task PopulateDropDown(AddDoctorViewModel model)
    {
        model.Specializations = await context.Specializations
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem
            {
                Value = s.SpecializationId.ToString(),
                Text = s.Name
            }).ToListAsync();


        model.Clinics = await context.Clinics
            .OrderBy(c => c.ClinicName)
            .Select(c => new SelectListItem
            {
                Value = c.ClinicId.ToString(),
                Text = c.ClinicName
            }).ToListAsync();
    }


    public async Task<IActionResult> ManageDoctors()
    {
        var doctors = await context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .Include(d => d.Clinic)
            .Select(d => new DoctorListItemViewModel
            {
                DoctorId = d.DoctorId,
                FullName = d.User.FullName,
                Email = d.User.Email,
                PhoneNumber = d.User.PhoneNumber,
                Specialization = d.Specialization.Name,
                Clinic = d.Clinic.ClinicName,
                ExperienceYears = d.ExperienceYears,
                Status = d.Status
            })
            .ToListAsync();
        return View(doctors);
    }


    [HttpGet]
    public async Task<IActionResult> DoctorDetails(int id)
    {
        var doctor = await context.Doctors
        .Include(d => d.User)
        .Include(d => d.Specialization)
        .Include(d => d.Clinic)
        .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        var viewModel = new DoctorDetailsViewModel
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.User.FullName,
            Email = doctor.User.Email,
            PhoneNumber = doctor.User.PhoneNumber,
            Gender = doctor.User.Gender,
            Specialization = doctor.Specialization.Name,
            Clinic = doctor.Clinic.ClinicName,
            Qualifications = doctor.Qualifications,
            ExperienceYears = doctor.ExperienceYears,
            Biography = doctor.Biography,
            Status = doctor.Status,
            ProfilePicturePath = doctor.ProfilePicturePath
        };

        return View(viewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateDoctor(int id)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        if (doctor.Status == AppStatuses.Active)
        {
            TempData["ErrorMessage"] = "Doctor is already active.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        doctor.Status = AppStatuses.Active;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Doctor activated successfully.";

        return RedirectToAction(nameof(ManageDoctors));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateDoctor(int id)
    {
        var doctor = await context.Doctors
            .FirstOrDefaultAsync(d => d.DoctorId == id);

        if (doctor is null)
        {
            TempData["ErrorMessage"] = "Doctor not found.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        if (doctor.Status == AppStatuses.Inactive)
        {
            TempData["ErrorMessage"] = "Doctor is already inactive.";
            return RedirectToAction(nameof(ManageDoctors));
        }

        doctor.Status = AppStatuses.Inactive;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Doctor deactivated successfully.";

        return RedirectToAction(nameof(ManageDoctors));
    }


    public async Task<IActionResult> ManagePatients()
    {
        var patients = await context.Patients

            .Include(p => p.User)

            .Select(p => new PatientListItemViewModel
            {
                PatientId = p.PatientId,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                Gender = p.User.Gender,
                DateOfBirth = p.DateOfBirth,
                BloodType = p.BloodType ?? "-"
            })

            .OrderBy(p => p.FullName)

            .ToListAsync();

        return View(patients);
    }


    public async Task<IActionResult> PatientDetails(int id)
    {
        var patient = await context.Patients

            .Include(p => p.User)

            .Where(p => p.PatientId == id)

            .Select(p => new PatientDetailsViewModel
            {
                PatientId = p.PatientId,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                Gender = p.User.Gender,
                DateOfBirth = p.DateOfBirth,
                BloodType = p.BloodType ?? "-",
                Allergies = p.Allergies ?? "-",
                ChronicDiseases = p.ChronicDiseases ?? "-",
                ProfilePicturePath = p.ProfilePicturePath
            })

            .SingleOrDefaultAsync();

        if (patient is null)
        {
            return NotFound();
        }

        return View(patient);
    }
}
