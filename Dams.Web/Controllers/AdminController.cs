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
}
