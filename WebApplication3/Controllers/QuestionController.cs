using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.QuestionViewModels;

namespace WebApplication3.Controllers
{
    public class QuestionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        public QuestionController(

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

        [HttpGet]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/{type}/")]
        public IActionResult Add(int testId, string type)
        {
            if (type == "SingleChoiceQuestion")
                return View("AddSingleChoiceQuestion");
            if (type == "MultiChoiceQuestion")
                return View("AddMultiChoiceQuestion");
            if (type == "TextQuestion")
                return View("AddTextQuestion");
            return View("AddSingleChoiceQuestion");
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testid}/Question/Add/SingleChoiceQuestion/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingleChoiceQuestion(AddSingleChoiceQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == model.TestId);
            if (test == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var q = new SingleChoiceQuestion { Title = model.Title, Test = test, QuestionType = model.QuestionType };
                await _context.Questions.AddAsync(q);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Test", new { id = model.TestId });
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testid}/Question/Add/TextQuestion/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTextQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == model.TestId);
            if (test == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var q = new TextQuestion { Title = model.Title, Test = test, QuestionType = model.QuestionType, TextRightAnswer = model.Text};
                await _context.Questions.AddAsync(q);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Test",new {id=model.TestId });
            }
            return View(model);
        }

        

    }
}