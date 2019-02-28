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
using Microsoft.EntityFrameworkCore.Internal;
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
        #region Поля
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        #endregion

        #region Конструктор
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
        #endregion

        #region Добавление
        [HttpGet]
        [Route("/Tests/Add/")]
        public IActionResult Add()
        {
            return View();
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
                var test = new Test { Name = model.Name, CreatedBy = user, IsEnabled = model.IsEnabled };
                await _context.Tests.AddAsync(test);

                // Добавить тест к пользователю, который его создал (чтобы он тоже мог проходить его)
                TestResult testResult = new TestResult
                {
                    IsCompleted = false,
                    Test = test,
                    CompletedByUser = user,
                    //TotalQuestions = (uint)test.Questions.Count()
                };
                user.TestResults.Add(testResult);


                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = test.Id });
            }
            return View(model);
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
                IsCompleted = false,
                Test = test,
                CompletedByUser = user,
                CompletedOn = DateTime.Now,
                //TotalQuestions = (uint)test.Questions.Count()
            };
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
            Response.StatusCode = 200;
            return RedirectToAction("Details", new { id = testId });
        }

        [HttpGet]
        [Authorize]
        [Route("/User/[controller]s/{testId}/AddTestToUser/")]
        public async Task<IActionResult> AddTestToUser(AddTestToUserViewModel model, int testId)
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
        public async Task<JsonResult> AddTestToUserAjax(int testId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var test = await _context.Tests.FindAsync(testId);
            if (user == null)
            {
                Response.StatusCode = 404;
                return new JsonResult("Пользователь с данным ID не найден");
            }
            if (test == null)
            {
                Response.StatusCode = 404;
                return new JsonResult("Тест с данным ID не найден");
            }
            if (!test.IsEnabled)
            {
                Response.StatusCode = 400;
                return new JsonResult("Тест не включен");
            }

            if (_context.TestResults.Any(t => t.CompletedByUser == user && t.Test == test))
            {
                Response.StatusCode = 400;
                return new JsonResult("Тест уже добавлен");
            }


            TestResult testResult = new TestResult
            {
                IsCompleted = false,
                Test = test,
                CompletedByUser = user
                //TotalQuestions = (uint)test.Questions.Count()
            };
            await _context.TestResults.AddAsync(testResult);
            await _context.SaveChangesAsync();
            Response.StatusCode = 200;
            return new JsonResult("Успешно");
        }
        #endregion

        #region Список тестов
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
        #endregion

        #region Детали
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

            var testResult = await _context.TestResults.Where(r => r.Test == test && r.CompletedByUser == user).FirstAsync();
            // у пользователя отсутствует тест
            if (testResult == null)
            {
                // добавляем тест к пользователю
                return RedirectToAction("AddTestToUser", new { testId = test.Id, userId = user.Id });
            }

            ViewData["testResult"] = testResult;
            return View(test);
        }
        #endregion

        #region Результаты
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
        #endregion

        #region Старт
        [Authorize]
        [HttpGet]
        [Route("/[controller]/Result/{testResultId}/Start/")]
        public async Task<IActionResult> Start(int testResultId)
        {
            ViewBag.IsStarted = false;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var testResult = await _context.TestResults.Include(t=>t.Test)
                .SingleAsync(tr => tr.Id == testResultId && tr.CompletedByUser == user);
            if (testResult == null) return NotFound();
            if (!testResult.Test.IsEnabled) return Forbid();
            if (_context.Answers.Any(a => a.TestResult == testResult))
            {
                ViewBag.IsStarted = true;
            }
            
            ViewBag.UserId = user.Id;
            ViewBag.QuestionsCount = _context.Questions.Count(q => q.Test==testResult.Test);
            return View(testResult);
        }
        
        // POST 
        [Authorize]
        [HttpPost]
        [Route("/[controller]/Result/{testResultId}/Start/")]
        public async Task<IActionResult> Start(int testResultId, ErrorViewModel model)
        {
            // TODO: пользователь начал проходить тест, переброс на первый вопрос
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var testResult = await _context.TestResults
                .Include(tr => tr.Test)
                    .ThenInclude(t=>t.Questions)
                .SingleAsync(t => t.Id == testResultId);
            if (testResult == null) return NotFound();
            if (!testResult.Test.IsEnabled) return Forbid();
            var questions = testResult.Test.Questions;
            if (questions.Count == 0) return NotFound();
            if (_context.Answers.Any(a => a.TestResult == testResult))
            {
                return RedirectToAction("Answer", "Answer", new {testResultId=testResult.Id, answerOrder=1});
            }
            List<Answer> answers = new List<Answer>();
            Answer answer = null;
            ushort order = 1;
            foreach (var question in questions)
            {
                switch (question.QuestionType)
                {
                    case "SingleChoiceQuestion":
                       answer = new SingleChoiceAnswer();
                       break;
                    case "MultiChoiceQuestion":
                        answer = new MultiChoiceAnswer();
                        break;
                    case "TextQuestion":
                        answer = new TextAnswer();
                        break;
                    case "DragAndDropQuestion":
                        answer = new DragAndDropAnswer();
                        break;                            
                }
                if (answer == null) throw new NullReferenceException();;
                answer.Question = question;
                answer.Score = 0;
                answer.TestResult = testResult;
                answer.Order = order;
                await _context.Answers.AddAsync(answer);
                answers.Add(answer);
                await _context.SaveChangesAsync();
                order++;
            }
            // answers.Shuffle()
           
            
            // TODO: redirect to first answer (question)
            //throw new NotImplementedException();
            return RedirectToAction("Answer", "Answer", new {testResultId=testResult.Id, answerOrder=1});
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        [Route("/Test/Result/{testResultId}/Finish/")]
        public async Task<IActionResult> FinishTest(int testResultId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var testResult = await _context.TestResults.SingleAsync(tr => tr.Id == testResultId);
            if (testResult.CompletedByUser != user) return BadRequest();
            if (testResult.IsCompleted) return BadRequest();
            testResult.IsCompleted = true;
            testResult.CompletedOn = DateTime.UtcNow;
            _context.Update(testResult);
            
            //var questions = testResult.Test.Questions.ToList();
            var answers = _context.Answers.Where(a => a.TestResult == testResult);
            
            foreach (var answer in answers)
            {
                if (answer is SingleChoiceAnswer singleChoiceAnswer)
                {
                    var question = 
                        await _context.SingleChoiceQuestions
                            .SingleAsync(q=>q.Id == singleChoiceAnswer.Question.Id);
                    singleChoiceAnswer.Score = (singleChoiceAnswer.Option == question.RightAnswer) ? question.Score : 0;

                    _context.SingleChoiceAnswers.Update(singleChoiceAnswer);
                }
                else if (answer is MultiChoiceAnswer multiChoiceAnswer)
                {
                    var question = await _context.MultiChoiceQuestions
                        .SingleAsync(q=>q.Id == multiChoiceAnswer.Question.Id);
                    
                    // TODO: Score
                    int counter=0;
                    multiChoiceAnswer.Score = 1;
                    foreach (var answerOption in multiChoiceAnswer.AnswerOptions)
                    {
                        if (answerOption.Checked != question.Options.Single(o=>o.Id == answerOption.OptionId).IsRight)
                        {
                            multiChoiceAnswer.Score = 0;
                            break;
                        }
                    }
                    

                    _context.MultiChoiceAnswers.Update(multiChoiceAnswer);
                }
                else if (answer is TextAnswer)
                {
                    
                }
                else if (answer is DragAndDropAnswer)
                {
                    
                }
            }
            //await _context.SaveChangesAsync();
            throw new NotImplementedException();
        }
    }
    #endregion
}