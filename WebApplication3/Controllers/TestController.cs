using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.TestViewModels;

namespace WebApplication3.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        
        public TestController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            // IEmailSender emailSender,
            //ISmsSender smsSender,
            ILoggerFactory loggerFactory
        )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
            //_smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<UserController>();
        }
        
        // GET
        
        
        [HttpGet]
       
        public IActionResult Add()
        {
            return View();
        }

        
        // GET
        [HttpGet]
        [Authorize]
        [Route("/[controller]s/")]
        public async Task<IActionResult> Tests()
        {
            //throw new NotImplementedException();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var createdTests = user.Tests;
            //if (createdTests == null) //return View(new ICollection<Test>);
            return View(createdTests);
            
        }
        
        [HttpPost]
        [Authorize]
        //[Route("Tests/{action=Add}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTestViewModel model)
        {
            
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (ModelState.IsValid)
            {
                var test = new Test {Name = model.Name, CreatedBy = user, IsEnabled = model.IsEnabled};
                await _context.Tests.AddAsync(test);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details",new {id=test.Id});
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        [Route("/[controller]s/{id}/")]
        public async Task<IActionResult> Details(int id)
        {
            var test = _context.Tests.SingleAsync(t => t.Id == id);
            throw new NotImplementedException();
        }
    }
}