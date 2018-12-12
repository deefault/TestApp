using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        [Route("/Tests/{testId}/Question/Add/{type}/", Name="Add")]
        public async Task<IActionResult> AddGet(int testId, int type)
        {
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();

            switch (type)
            {
                case (int) Question.QuestionTypeEnum.SingleChoiceQuestion:
                    return View("AddSingleChoiceQuestion");
                case (int) Question.QuestionTypeEnum.MultiChoiceQuestion:
                    return View("AddMultiChoiceQuestion");
                case (int) Question.QuestionTypeEnum.TextQuestion:
                    return View("AddTextQuestion");
                default:
                    return View("AddSingleChoiceQuestion");
            }
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Single/", Name = "AddSingle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingleChoiceQuestion([FromBody]AddSingleChoiceQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null)
            {
            }

            if (test.CreatedBy != user)
            {
                return Forbid();
            }
            
            model.TestId = test.Id;
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    List<Option> options= new List<Option>();
                    var question = new SingleChoiceQuestion
                    {
                        Title = model.Title,QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 1),Test  = test
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    foreach (var option in model.Options)
                    {
                        // добавить в базу Options
                        var optionCreated = (await _context.AddAsync(
                            new Option{IsRight = option.IsRight,Text = option.Text,Question = questionCreated})).Entity;
                        //questionCreated.Options.Add(optionCreated);
                       
                        if (optionCreated.IsRight) questionCreated.RightAnswer = optionCreated;
                    }
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync(); 
                    ts.Commit();
                }
                
                
                return new JsonResult(model);
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(model);
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Multi/", Name = "AddMulti")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMultiChoiceQuestion([FromBody] AddMultiChoiceQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null)
            {
                return NotFound();
            }

            if (test.CreatedBy != user)
            {
                return Forbid();
            }
            
            model.TestId = test.Id;
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    List<Option> options= new List<Option>();
                    var question = new SingleChoiceQuestion
                    {
                        Title = model.Title,QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 2),Test  = test
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    foreach (var option in model.Options)
                    {
                        // добавить в базу Options
                        var optionCreated = (await _context.AddAsync(
                            new Option{IsRight = option.IsRight,Text = option.Text,Question = questionCreated})).Entity;   
                    }
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync(); 
                    ts.Commit();
                }
                
                
                return new JsonResult(model);
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(model);
        }
        
        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Text/", Name = "AddText")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTextQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null)
            {
                return NotFound();
            }
            if (test.CreatedBy != user)
            {
                return Forbid();
            }
            if (ModelState.IsValid)
            {
                var q = new TextQuestion { Title = model.Title, Test = test, QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 3), TextRightAnswer = model.Text};
                await _context.Questions.AddAsync(q);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Test",new {id=model.TestId });
            }
            return View(model);
        }
    }
}