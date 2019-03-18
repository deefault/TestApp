using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.QuestionViewModels;

namespace WebApplication3.Controllers
{
    public class QuestionController : Controller
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
        #endregion

        #region Добавление GET
        [HttpGet]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/{type}/", Name = "Add")]
        public async Task<IActionResult> AddGet(int testId, int type)
        {
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();

            switch (type)
            {
                case (int)Question.QuestionTypeEnum.SingleChoiceQuestion:
                    return View("AddSingleChoiceQuestion",new AddSingleChoiceQuestionViewModel());
                case (int)Question.QuestionTypeEnum.MultiChoiceQuestion:
                    return View("AddMultiChoiceQuestion",new AddMultiChoiceQuestionViewModel());
                case (int)Question.QuestionTypeEnum.TextQuestion:
                    return View("AddTextQuestion",new AddTextQuestionViewModel());
                case (int)Question.QuestionTypeEnum.DragAndDropQuestion:
                    return View("AddDragAndDropQuestion",new AddDragAndDropQuestionViewModel());
                case (int)Question.QuestionTypeEnum.CodeQuestion:
                    return View("AddCodeQuestion", new AddCodeQuestionViewModel());
                default:
                    return View("AddSingleChoiceQuestion",new AddSingleChoiceQuestionViewModel());
            }
        }
        #endregion

        #region Добавление POST
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
                    var question = new SingleChoiceQuestion
                    {
                        Title = model.Title,
                        QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 1),
                        Test = test,
                        Score = model.Score
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    foreach (var option in model.Options)
                    {
                        // добавить в базу Options
                        var optionCreated = (await _context.AddAsync(
                            new Option { IsRight = option.IsRight, Text = option.Text, Question = questionCreated })).Entity;
                        //questionCreated.Options.Add(optionCreated);

                        if (optionCreated.IsRight) questionCreated.RightAnswer = optionCreated;
                    }
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
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
                    var question = new MultiChoiceQuestion
                    {
                        Title = model.Title,
                        QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 2),
                        Test = test,
                        Score = model.Score
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    foreach (var option in model.Options)
                    {
                        // добавить в базу Options
                        var optionCreated = (await _context.AddAsync(
                            new Option { IsRight = option.IsRight, Text = option.Text, Question = questionCreated })).Entity;
                    }
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/DragAndDrop/", Name = "AddDragAndDrop")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDragAndDropQuestion([FromBody] AddDragAndDropQuestionViewModel model)
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
                    var question = new DragAndDropQuestion
                    {
                        Title = model.Title,
                        QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 4),
                        Test = test,
                        Score = model.Score
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.Questions.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    foreach (var option in model.Options)
                    {
                        // добавить в базу Options
                        var optionCreated = (await _context.Options.AddAsync(
                            new Option { Order = option.Order, Text = option.Text, Question = questionCreated })).Entity;
                    }
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }


                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Text/", Name = "AddText")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTextQuestion([FromBody] AddTextQuestionViewModel model)
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
                    var question = new TextQuestion
                    {
                        Title = model.Title,
                        QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 3),
                        Test = test,
                        Score = model.Score
                    };
                    //создать в базе вопрос
                    var questionCreated = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync(); //применить изменения
                    var option = model.Options[0];
                    // добавить в базу Options
                    var optionCreated = (await _context.AddAsync(
                        new Option { Text = option.Text, Question = questionCreated })).Entity;
                    //questionCreated.Options.Add(optionCreated);
                    questionCreated.TextRightAnswer = optionCreated.Text;
                    // обновить вопрос и применить изменения
                    _context.Questions.Update(questionCreated);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }
        #endregion

        #region Редактирование GET
        [HttpGet]
        [Authorize]
        [Route("/Tests/{testId}/Question/{questionId}/Edit/")]
        public async Task<IActionResult> Edit(int testId, int questionId)
        {
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.Questions
                .Include(q => q.Options)
                .SingleOrDefaultAsync(q => q.Id == questionId);
            if (question == null) return NotFound();
            if (question.Test != test) return NotFound();

            switch (question.QuestionType)
            {
                // TODO Edit pages
                case nameof(Question.QuestionTypeEnum.SingleChoiceQuestion):
                    return View("EditSingleChoiceQuestion", question);
                case nameof(Question.QuestionTypeEnum.MultiChoiceQuestion):
                    return View("EditMultiChoiceQuestion", question);
                case nameof(Question.QuestionTypeEnum.TextQuestion):
                    return View("EditTextQuestion", question);
                case nameof(Question.QuestionTypeEnum.DragAndDropQuestion):
                    question.Options = question.Options.OrderBy(o => o.Order).ToList();
                    return View("EditDragAndDropQuestion", question);
                default:
                    return View("EditSingleChoiceQuestion", question);
            }
        }
        #endregion

        #region Редактирование POST
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/SingleChoiceQuestion/{questionId}/Edit/", Name = "EditSingle")]
        public async Task<IActionResult> EditSingleChoiceQuestion(int testId, int questionId,
            [FromBody] AddSingleChoiceQuestionViewModel model)
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
            var question = await _context.SingleChoiceQuestions
                .Include(q => q.Options)
                .SingleAsync(q => q.Id == questionId);
            if (question.Test != test)
            {
                return NotFound();
            }

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    //обновить опшены
                    UpdateQuestionOptions(model.Options, question);
                    // обновить вопрос и применить изменения
                    question.RightAnswer = question.Options.Single(o => o.IsRight);
                    question.Title = model.Title;
                    question.Score = model.Score;

                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/MultiChoiceQuestion/{questionId}/Edit/", Name = "EditMulti")]
        public async Task<IActionResult> EditMultiChoiceQuestion(int testId, int questionId,
            [FromBody] AddMultiChoiceQuestionViewModel model)
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
            var question = await _context.MultiChoiceQuestions
                .Include(q => q.Options)
                .SingleAsync(q => q.Id == questionId);
            if (question.Test != test)
            {
                return NotFound();
            }

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    //обновить опшены
                    UpdateQuestionOptions(model.Options, question);
                    // обновить вопрос и применить изменения
                    question.Title = model.Title;
                    question.Score = model.Score;
                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/TextQuestion/{questionId}/Edit/", Name = "EditText")]
        public async Task<IActionResult> EditTextQuestion(int testId, int questionId, [FromBody] AddTextQuestionViewModel model)
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
            var question = await _context.TextQuestions
                .Include(q => q.Options)
                .SingleAsync(q => q.Id == questionId);
            if (question.Test != test)
            {
                return NotFound();
            }

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    //обновить опшены
                    UpdateQuestionOptions(model.Options, question);
                    // обновить вопрос и применить изменения
                    question.TextRightAnswer = question.Options.Single().Text;
                    question.Title = model.Title;
                    question.Score = model.Score;
                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }
  
                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/DragAndDropQuestion/{questionId}/Edit/", Name = "EditDragAndDrop")]
        public async Task<IActionResult> EditDragAndDropQuestion(int testId, int questionId, [FromBody] AddDragAndDropQuestionViewModel model)
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
            var question = await _context.DragAndDropQuestions
                .Include(q => q.Options)
                .SingleAsync(q => q.Id == questionId);
            if (question.Test != test)
            {
                return NotFound();
            }

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    //обновить опшены
                    UpdateDragAndDropQuestionOptions(model.Options, question);
                    // обновить вопрос и применить изменения
                    question.Title = model.Title;
                    question.Score = model.Score;
                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }
            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error);
                }
            }

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/Question/{questionId}/Delete/")]
        public async Task<IActionResult> Delete(int testId, int questionId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.Questions
                .SingleOrDefaultAsync(q => q.Id == questionId);
            if (question == null) return NotFound();
            if (question.Test != test) return NotFound();
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Test", new { id = testId });
        }
        #endregion

        #region Детали
        [HttpGet]
        [Authorize]
        [Route("/Tests/{testId}/Question/{questionId}/Details/")]
        public async Task<IActionResult> Details(int testId, int questionId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == testId);
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.Questions
                .Include(q => q.Options)
                .SingleOrDefaultAsync(q => q.Id == questionId);
            if (question == null) return NotFound();
            if (question.QuestionType == "DragAndDropQuestion")
                question.Options = question.Options.OrderBy(o => o.Order).ToList();
            if (question.Test != test) return NotFound();
            return View(question);

        }
        #endregion

        #region Вспомогательные методы
        private async void UpdateQuestionOptions(List<OptionViewModel> options, Question question)
        {

            var optionsToCreate = new List<OptionViewModel>();
            var otherOptions = new List<OptionViewModel>();
            var optionsToUpdate = new List<Option>();
            var optionsToDelete = new List<Option>();


            foreach (var option in options)
            {
                if (option.Id == null) optionsToCreate.Add(option);
                else otherOptions.Add(option);
            }

            List<int?> optionsIds = otherOptions.Select(o => o.Id).ToList();

            optionsToUpdate = question.Options.Where(o => optionsIds.Contains(o.Id)).ToList();
            optionsToDelete = question.Options.Where(o => !optionsIds.Contains(o.Id)).ToList();

            foreach (var option in optionsToUpdate)
            {
                var optionData = options.Single(o => o.Id == option.Id);
                option.IsRight = optionData.IsRight;
                option.Text = optionData.Text;
                _context.Update(option);
            }

            await _context.SaveChangesAsync();

            foreach (var option in optionsToDelete)
            {
                _context.Options.Remove(option);
            }

            await _context.SaveChangesAsync();

            foreach (var option in optionsToCreate)
            {
                var o = new Option { Question = question, IsRight = option.IsRight, Text = option.Text };
                _context.Options.Add(o);
            }
            await _context.SaveChangesAsync();

        }

        private async void UpdateDragAndDropQuestionOptions(List<OptionViewModel> options, Question question)
        {

            var optionsToCreate = new List<OptionViewModel>();
            var otherOptions = new List<OptionViewModel>();
            var optionsToUpdate = new List<Option>();
            var optionsToDelete = new List<Option>();


            foreach (var option in options)
            {
                if (option.Id == null) optionsToCreate.Add(option);
                else otherOptions.Add(option);
            }

            List<int?> optionsIds = otherOptions.Select(o => o.Id).ToList();

            optionsToUpdate = question.Options.Where(o => optionsIds.Contains(o.Id)).ToList();
            optionsToDelete = question.Options.Where(o => !optionsIds.Contains(o.Id)).ToList();
            foreach (var option in optionsToUpdate)
            {
                var optionData = options.Single(o => o.Id == option.Id);
                option.IsRight = optionData.IsRight;
                option.Text = optionData.Text;
                option.Order = optionData.Order;
                _context.Update(option);
            }

            await _context.SaveChangesAsync();

            foreach (var option in optionsToDelete)
            {
                _context.Options.Remove(option);
            }

            await _context.SaveChangesAsync();

            foreach (var option in optionsToCreate)
            {
                var o = new Option { Order = option.Order, Question = question, IsRight = option.IsRight, Text = option.Text };
                _context.Options.Add(o);
            }
            await _context.SaveChangesAsync();

        }
        #endregion
    }
}