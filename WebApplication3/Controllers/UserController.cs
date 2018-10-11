using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    public class UserController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return
            View();
        }
    }
}