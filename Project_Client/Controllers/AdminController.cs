using Microsoft.AspNetCore.Mvc;

namespace Project_Client.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Login() 
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            ViewData["ShowHeader"] = false;
            return View();
        }
        public IActionResult IndexProduct()
        {
            ViewData["ShowHeader"] = false;
            return View();
        }
        public IActionResult IndexCategory()
        {
            ViewData["ShowHeader"] = false;
            return View();
        }

        public IActionResult IndexUser()
        {
            ViewData["ShowHeader"] = false;
            return View();
        }
    }
}
