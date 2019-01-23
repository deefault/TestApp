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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        [Route("/Tests/Add/")]
        public IActionResult Add()
        {
            return View();
        }

        
        // GET
        [HttpGet]
        [Authorize]
        [Route("/Tests/")]
        public async Task<IActionResult> Tests()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var createdTests = _context.Tests.Where(t=>t.CreatedBy.Id == user.Id).ToList();
            //if (createdTests == null) //return View(new ICollection<Test>);
            return View(createdTests);
            
        }
        
        // GET
        [HttpGet]
        [Authorize]
        [Route("/Tests/Results/")]
        public async Task<IActionResult> TestResults()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var tests = _context.TestResults.Where(t=>t.CompletedByUser == user)
                .Include(a=>a.Test).ThenInclude(b=>b.CreatedBy).ToList();
            return View(tests);
        }
        
        [HttpPost]
        [Authorize]
        [Route("/Tests/Add/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTestViewModel model)
        {  
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (ModelState.IsValid)
            {
                var test = new Test {Name = model.Name, CreatedBy = user, IsEnabled = model.IsEnabled};
                await _context.Tests.AddAsync(test);
                
                // Добавить тест к пользователю, который его создал (чтобы он тоже мог проходить его)
                TestResult testResult = new TestResult
                {
                    isCompleted = false, 
                    Test = test,
                    CompletedByUser = user,
                    //TotalQuestions = (uint)test.Questions.Count()
                };
                user.TestResults.Add(testResult);
                
                
                await _context.SaveChangesAsync();
                return RedirectToAction("Details",new {id=test.Id});
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        [Route("/Tests/{id}/")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.Include(t => t.Questions).SingleAsync(t => t.Id == id);
            var questions = await _context.Questions.Where(q => q.Test == test).ToListAsync();
            if (test == null)
            {
                return NotFound();
            }
            ViewData["user"] = user;
            ViewData["question"] = questions;
            if (test.CreatedBy == user)
            {    
                return View(test);
            }
            else
            {
                var testResult = await _context.TestResults.Where(r => r.Test == test || r.CompletedByUser == user).FirstAsync();
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

        [HttpPost]
        [Authorize]
        [Route("/User/[controller]s/{testId}/AddTestToUser/")]
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
                CompletedOn = DateTime.Now,
                //TotalQuestions = (uint)test.Questions.Count()
            };
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
            Response.StatusCode = 200;
            return RedirectToAction("Details", new {id = testId});
        }
        
        [HttpGet]
        [Authorize]
        [Route("/User/[controller]s/{testId}/AddTestToUser/")]
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
        
        [HttpPost]
        [Authorize]
        [Route("/User/[controller]s/AddTestToUserAjax/")]
        public async Task<JsonResult> AddTestToUserAjax(int testId)
        {
            // TODO:
            //throw new NotImplementedException();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            if (test == null)
            {
                Response.StatusCode = 404;
                return new JsonResult("Тест с данным ID не найден");
            }
            if (!test.IsEnabled)
            {
                Response.StatusCode =400;
                return new JsonResult("Тест не включен");
            }

            if (_context.TestResults.Any(t => t.CompletedByUser==user && t.Test == test))
            {
                Response.StatusCode =400;
                return new JsonResult("Тест уже добавлен");
            }
            
            
            
            TestResult testResult = new TestResult
            {
                isCompleted = false, 
                Test = test,
                CompletedByUser = user
                //TotalQuestions = (uint)test.Questions.Count()
            };
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
            Response.StatusCode = 200;
            return new JsonResult("Успешно");
        }

        
        // GET /User/Tests/<id>/Start/
        [Authorize]
        [HttpGet]
        [Route("/User/[controller]s/{testId}/Start/")]
        public Task<IActionResult> Start(int testId, ErrorViewModel model)
        {
            // TODO: страница с информацией и поддтверждением начала теста (post запрос на этот же url)
            throw new NotImplementedException();
        }
        
        // POST /User/Tests/<id>/Start/
        [Authorize]
        [HttpPost]
        [Route("/User/[controller]s/{testId}/Start/")]
        public Task<IActionResult> Start(int testId)
        {
            // TODO: пользователь начал проходить тест, переброс на первый вопрос
            throw new NotImplementedException();
        }
    }
}