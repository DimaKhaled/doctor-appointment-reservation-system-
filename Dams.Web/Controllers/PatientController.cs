using Dams.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dams.Web.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class PatientController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
