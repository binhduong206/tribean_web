// Controllers/Admin/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Tribean.Routers.Admin;

namespace Tribean.Controllers.Admin
{
    public class DashboardController : Controller
    {
        [HttpGet(DashboardRouter.Index)]
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["PageTitle"] = "Dashboard";
            ViewData["Breadcrumb"] = "Overview";
            return View("~/Views/Admin/Pages/Dashboard.cshtml");
        }
    }
}