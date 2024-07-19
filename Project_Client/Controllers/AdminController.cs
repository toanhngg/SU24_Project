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
            return View();
        }
        public IActionResult IndexProduct()
        {
            return View();
        }
        public IActionResult IndexCategory()
        {
            return View();
        }
    }
}
