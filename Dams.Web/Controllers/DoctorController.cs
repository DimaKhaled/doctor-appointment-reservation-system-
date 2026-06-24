using Dams.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dams.Web.Controllers;

[Authorize(Roles = AppRoles.Doctor)]
public class DoctorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
