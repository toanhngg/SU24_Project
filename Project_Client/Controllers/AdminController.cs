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
    }
}
