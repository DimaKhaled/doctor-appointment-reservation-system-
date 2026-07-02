using System.Diagnostics;
using Dams.Web.Data;
using Dams.Web.Models;
using Dams.Web.ViewModels.Patient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DamsDbContext _context;

    public HomeController(ILogger<HomeController> logger, DamsDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.Patient))
                return RedirectToAction("Index", "Patient");

            if (User.IsInRole(AppRoles.Doctor))
                return RedirectToAction("Index", "Doctor");

            if (User.IsInRole(AppRoles.Admin))
                return RedirectToAction("Index", "Admin");
        }

        var specializations = await _context.Specializations
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        var featuredDoctors = await _context.Doctors
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

        ViewBag.Specializations = specializations;
        ViewBag.FeaturedDoctors = featuredDoctors;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}