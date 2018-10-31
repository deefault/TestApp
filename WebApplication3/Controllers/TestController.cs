using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Models.TestViewModels;

namespace WebApplication3.Controllers
{
    public class TestController : Controller
    {
        // GET
        [HttpGet]
        //[Route("/User/UserName/Tests/",)]
        public IActionResult Index()
        {
            throw new NotImplementedException();
        }
        
        [HttpGet]
        [Route("Test/{action=Add}/")]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Route("Tests/{action=Add}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTestViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}