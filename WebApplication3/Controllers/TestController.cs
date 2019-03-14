using System;
using System.IO;
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
using Microsoft.AspNetCore.Http;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace WebApplication3.Controllers
{
    public partial class TestController : Controller
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
        public async Task<IActionResult> Add(AddTestModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (ModelState.IsValid)
            {
                var test = new Test { Name = model.Model2.Name, CreatedBy = user, IsEnabled = model.Model2.IsEnabled };
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

        [HttpGet]
        [Route("/Tests/AddFromFile/")]
        public IActionResult AddFromFile()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/AddFromFile/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFromFile(IFormFile uploadedFile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                // get a stream
                var stream = uploadedFile.OpenReadStream();
                TestData testData = Parser.Parse(Tokenizer.Tokenize(new StreamReader(stream)));
                testData.Test.CreatedBy = user;
                await _context.Tests.AddAsync(testData.Test);
                TestResult testResult = new TestResult
                {
                    IsCompleted = false,
                    Test = testData.Test,
                    CompletedByUser = user,
                };
                user.TestResults.Add(testResult);
                await _context.SaveChangesAsync();
                foreach (var q in testData.Questions)
                {
                    await _context.Questions.AddAsync(q);
                }
                await _context.SaveChangesAsync();
                foreach (var o in testData.Options)
                {
                    await _context.Options.AddAsync(o);
                    if (o.Question is SingleChoiceQuestion && o.IsRight)
                    {
                        var questionCreated = _context.Questions.Single(q => q.Id == o.Question.Id) as SingleChoiceQuestion;
                        questionCreated.RightAnswer = o;
                        _context.Questions.Update(questionCreated);
                    }
                }

                // Добавить тест к пользователю, который его создал (чтобы он тоже мог проходить его)

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = testData.Test.Id });
            }
            catch (Exception e)
            {
                ViewBag.Exception = e.Message;
                return View();
            }
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

        #region Удаление
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/Delete/")]
        public async Task<IActionResult> Delete(int testId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            if (test.CreatedBy != user) return Forbid();
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction("Tests");
        }
        #endregion

        #region Список тестов
        [HttpGet]
        [Authorize]
        [Route("/Tests/")]
        public async Task<IActionResult> Tests()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var createdTests = _context.Tests.Where(t => t.CreatedBy.Id == user.Id).ToList();
            //if (createdTests == null) //return View(new ICollection<Test>);
            AddTestModel addTestModel = new AddTestModel();
            addTestModel.Model1 = createdTests;
            addTestModel.Model2 = new AddTestViewModel();
            return View(addTestModel);

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
            var tests = _context.TestResults.Where(t => t.CompletedByUser == user)
                .Include(a => a.Test).ThenInclude(b => b.CreatedBy).ToList();
            return View(tests);
        }
        [HttpGet]
        [Authorize]
        [Route("/[controller]/Result/{testResultId}/Details/")]
        public async Task<IActionResult> TRDetails(int testResultId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var testResult = await _context.TestResults.Include(tr => tr.Test)
                .SingleAsync(tr => tr.Id == testResultId && tr.CompletedByUser == user);
            if (testResult == null) return NotFound();
            if (!testResult.Test.IsEnabled || !testResult.IsCompleted) return Forbid();
            return View(testResult);
        }
        #endregion

        #region Прохождение
        [Authorize]
        [HttpGet]
        [Route("/[controller]/Result/{testResultId}/Start/")]
        public async Task<IActionResult> Start(int testResultId)
        {
            ViewBag.IsStarted = false;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var testResult = await _context.TestResults.Include(t => t.Test)
                .SingleAsync(tr => tr.Id == testResultId && tr.CompletedByUser == user);
            if (testResult == null) return NotFound();
            if (!testResult.Test.IsEnabled) return Forbid();
            if (_context.Answers.Any(a => a.TestResult == testResult))
            {
                ViewBag.IsStarted = true;
            }

            ViewBag.UserId = user.Id;
            ViewBag.QuestionsCount = _context.Questions.Count(q => q.Test == testResult.Test);
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
                    .ThenInclude(t => t.Questions)
                .SingleAsync(t => t.Id == testResultId);
            if (testResult == null) return NotFound();
            if (!testResult.Test.IsEnabled) return Forbid();
            var questions = testResult.Test.Questions;
            if (questions.Count == 0) return NotFound();
            if (_context.Answers.Any(a => a.TestResult == testResult))
            {
                return RedirectToAction("Answer", "Answer", new { testResultId = testResult.Id, answerOrder = 1 });
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
                if (answer == null) throw new NullReferenceException();
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
            return RedirectToAction("Answer", "Answer", new { testResultId = testResult.Id, answerOrder = 1 });
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
            uint count = 0;
            //var questions = testResult.Test.Questions.ToList();
            var answers = _context.Answers.Where(a => a.TestResult == testResult);
            testResult.TotalQuestions = (uint)answers.Count();
            foreach (var answer in answers)
            {
                if (answer is SingleChoiceAnswer)
                {
                    var singleChoiceAnswer = await _context.SingleChoiceAnswers
                        .Include(a => a.Question).Include(a => a.Option)
                        .SingleAsync(a => a.Id == answer.Id);
                    var question =
                        await _context.SingleChoiceQuestions
                            .SingleAsync(q => q.Id == singleChoiceAnswer.QuestionId);
                    singleChoiceAnswer.Score = 0;
                    if (singleChoiceAnswer.Option != null)
                        if (singleChoiceAnswer.Option == question.RightAnswer)
                        {
                            singleChoiceAnswer.Score = question.Score;
                            count++;
                        }
                    //singleChoiceAnswer.Score = (singleChoiceAnswer.Option == question.RightAnswer) ? question.Score : 0;

                    _context.SingleChoiceAnswers.Update(singleChoiceAnswer);
                }
                else if (answer is MultiChoiceAnswer)
                {
                    var multiChoiceAnswer = await _context.MultiChoiceAnswers
                        .Include(a => a.AnswerOptions).Include(a => a.Question).ThenInclude(q => q.Options)
                        .SingleAsync(a => a.Id == answer.Id);
                    var question =
                        await _context.MultiChoiceQuestions
                            .SingleAsync(q => q.Id == multiChoiceAnswer.QuestionId);

                    // TODO: Score
                    //int counter = 0;
                    multiChoiceAnswer.Score = question.Score;
                    count++;
                    if (multiChoiceAnswer.AnswerOptions == null || multiChoiceAnswer.AnswerOptions.Count == 0)
                        count--;
                    foreach (var answerOption in multiChoiceAnswer.AnswerOptions)
                    {
                        if (answerOption.Checked != question.Options.Single(o => o.Id == answerOption.OptionId).IsRight)
                        {
                            multiChoiceAnswer.Score = 0;
                            count--;
                            break;
                        }
                    }


                    _context.MultiChoiceAnswers.Update(multiChoiceAnswer);
                }
                else if (answer is TextAnswer textAnswer)
                {
                    var question =
                        await _context.TextQuestions
                            .SingleAsync(q => q.Id == textAnswer.QuestionId);
                    if (textAnswer.Text == question.TextRightAnswer)
                    {
                        textAnswer.Score = question.Score;
                        count++;
                    }
                    else
                        textAnswer.Score = 0;
                    //textAnswer.Score = (textAnswer.Text == question.TextRightAnswer) ? question.Score : 0;
                    _context.TextAnswers.Update(textAnswer);
                }
                else if (answer is DragAndDropAnswer)
                {
                    var dndAnswer = await _context.DragAndDropAnswers
                    .Include(a => a.Question).Include(a => a.DragAndDropAnswerOptions)
                    .SingleAsync(a => a.Id == answer.Id);
                    var question =
                        await _context.DragAndDropQuestions
                            .SingleAsync(q => q.Id == dndAnswer.QuestionId);
                    dndAnswer.Score = question.Score;
                    count++;
                    if (dndAnswer.DragAndDropAnswerOptions == null || dndAnswer.DragAndDropAnswerOptions.Count == 0)
                        count--;
                    foreach (var dndOption in dndAnswer.DragAndDropAnswerOptions)
                    {
                        if (dndOption.RightOptionId != dndOption.OptionId)
                        {
                            dndAnswer.Score = 0;
                            count--;
                            break;
                        }
                    }
                }
            }
            testResult.RightAnswersCount = count;
            _context.Update(testResult);
            await _context.SaveChangesAsync();
            //throw new NotImplementedException();
            return RedirectToAction("TestResults");
        }
        #endregion
    }
}