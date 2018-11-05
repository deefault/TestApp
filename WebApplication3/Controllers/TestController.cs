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
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2.HPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.TestViewModels;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

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
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleAsync(t => t.Id == id);
            if (test == null)
            {
                Response.StatusCode = 404;
                return View();
            }
            ViewData["user"] = user;
            if (test.CreatedBy == user)
            {
                
                return View(test);
            }
            else
            {
                var testResult = _context.TestResults.Where(r => r.Test == test || r.CompletedByUser == user).FirstAsync();
                // у пользователя отсутствует тест
                if (testResult == null)
                {
                    // добавляем тест к пользователю
                    return RedirectToAction("AddTestToUser",new {testId = test.Id, userId=user.Id});
                }

                ViewData["testResult"] = testResult;
                return View(test);
            }
            return View(test);

            

        }

        [HttpGet]
        [Authorize]
        [Route("User/[controller]s/{testId}/AddTestToUser/")]
        public async Task<IActionResult> AddTestToUser(int testId)
        {
            //throw new NotImplementedException();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleAsync(t => t.Id == testId);
            if (test == null)
            {
                return NotFound();
            }
            TestResult testResult = new TestResult
            {
                isCompleted = false, 
                Test = test,
                CompletedByUser = user,
                CompletedOn = DateTime.Now
            };
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
            Response.StatusCode = 200;
            return RedirectToAction("Details", new {id = testId});
        }
        
        [HttpPost]
        [Authorize]
        [Route("User/[controller]s/{testId}/AddTestToUser/")]
        public async Task<IActionResult> AddTestToUser(AddTestToUserViewModel model,int testId)
        {
            var test = await _context.Tests.SingleAsync(t => t.Id == testId);
            if (test == null)
            {
                Response.StatusCode = 404;
                return NotFound();
            }

            ViewData["test"] = test;
            return View(model);
        }
    }
}