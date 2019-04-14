using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Models.QuestionViewModels;

namespace WebApplication3.Controllers
{
    public class QuestionController : Controller
    {
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
                    return View("AddSingleChoiceQuestion", new AddSingleChoiceQuestionViewModel());
                case (int)Question.QuestionTypeEnum.MultiChoiceQuestion:
                    return View("AddMultiChoiceQuestion", new AddMultiChoiceQuestionViewModel());
                case (int)Question.QuestionTypeEnum.TextQuestion:
                    return View("AddTextQuestion", new AddTextQuestionViewModel());
                case (int)Question.QuestionTypeEnum.DragAndDropQuestion:
                    return View("AddDragAndDropQuestion", new AddDragAndDropQuestionViewModel());
                case (int)Question.QuestionTypeEnum.CodeQuestion:
                    return View("AddCodeQuestion", new AddCodeQuestionViewModel());
                default:
                    return View("AddSingleChoiceQuestion", new AddSingleChoiceQuestionViewModel());
            }
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
                case nameof(Question.QuestionTypeEnum.CodeQuestion):
                    var codeQuestion = await _context.CodeQuestions.SingleOrDefaultAsync(q => q.Id == questionId);
                    codeQuestion.Code = await _context.Codes.SingleOrDefaultAsync(c => c.Question == codeQuestion);
                    return View("EditCodeQuestion", codeQuestion);
                default:
                    return View("EditSingleChoiceQuestion", question);
            }
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

        #region Поля

        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly SignInManager<User> _signInManager;

        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

        #endregion

        #region Добавление POST

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Single/", Name = "AddSingle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSingleChoiceQuestion([FromBody] AddSingleChoiceQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null) return NotFound();

            if (test.CreatedBy != user) return Forbid();

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
                                new Option { IsRight = option.IsRight, Text = option.Text, Question = questionCreated }))
                            .Entity;
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
                foreach (var error in modelState.Errors)
                    errors.Add(error);
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
            if (test == null) return NotFound();

            if (test.CreatedBy != user) return Forbid();

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
                                new Option { IsRight = option.IsRight, Text = option.Text, Question = questionCreated }))
                            .Entity;
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
                foreach (var error in modelState.Errors)
                    errors.Add(error);
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
            if (test == null) return NotFound();

            if (test.CreatedBy != user) return Forbid();

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
                foreach (var error in modelState.Errors)
                    errors.Add(error);
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
            if (test == null) return NotFound();

            if (test.CreatedBy != user) return Forbid();

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
                foreach (var error in modelState.Errors)
                    errors.Add(error);
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Code/", Name = "AddCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCodeQuestion([FromBody] AddCodeQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null) return NotFound();

            if (test.CreatedBy != user) return Forbid();
            model.TestId = test.Id;
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                using (var ts = _context.Database.BeginTransaction())
                {
                    var question = new CodeQuestion
                    {
                        Title = model.Title,
                        QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), 5),
                        Test = test,
                        Score = model.Score
                    };
                    question = (await _context.AddAsync(question)).Entity;
                    await _context.SaveChangesAsync();
                    var co = new Code
                        {
                            Args = model.Code.Args,
                            Question = question,
                            Value = model.Code.Value
                        };
                    var code = (await _context.AddAsync(
                        new Code
                        {
                            Args = model.Code.Args,
                            Question = question,
                            Value = model.Code.Value,
                            Output = Compile(co)
                        })).Entity;
                    await _context.AddAsync(
                        new Option { Text = model.Code.Output, Question = question });
                    question.Code = code;
                    try
                    {
                        _context.Remove(await _context.Codes.SingleAsync(c => c.Test == test));
                    }
                    catch
                    {
                    }

                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
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
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.SingleChoiceQuestions
                .AsNoTracking()
                .SingleAsync(q => q.Id == questionId);

            if (question.TestId != test.Id) return NotFound();

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    // copy question
                    question.Id = 0;

                    _context.SingleChoiceQuestions.Add(question);
                    await _context.SaveChangesAsync();
                    var questionCopyId = question.Id;

                    // заархивировать
                    var questionOld = await _context.SingleChoiceQuestions
                        .SingleAsync(q => q.Id == questionId);
                    questionOld.IsDeleted = true;
                    _context.SingleChoiceQuestions.Update(questionOld);
                    await _context.SaveChangesAsync();

                    UpdateQuestionOptions(model.Options, questionCopyId);
                    //обновить опшены и копию
                    var questionCopy = await _context.SingleChoiceQuestions
                        .Include(q => q.Options)
                        .SingleAsync(q => q.Id == questionCopyId);
                    questionCopy.Title = model.Title;
                    questionCopy.Score = model.Score;
                    questionCopy.RightAnswer = questionCopy.Options.Single(o => o.IsRight);
                    _context.SingleChoiceQuestions.Update(questionCopy);


                    await _context.SaveChangesAsync();

                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);

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
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.MultiChoiceQuestions
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question.TestId != test.Id) return NotFound();

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    // copy question
                    question.Id = 0;

                    _context.MultiChoiceQuestions.Add(question);
                    await _context.SaveChangesAsync();
                    var questionCopyId = question.Id;

                    // заархивировать
                    var questionOld = await _context.MultiChoiceQuestions
                        .SingleAsync(q => q.Id == questionId);
                    questionOld.IsDeleted = true;
                    _context.MultiChoiceQuestions.Update(questionOld);
                    await _context.SaveChangesAsync();

                    //обновить опшены и копию
                    var questionCopy = await _context.MultiChoiceQuestions
                        .Include(q => q.Options)
                        .SingleAsync(q => q.Id == questionCopyId);
                    questionCopy.Title = model.Title;
                    questionCopy.Score = model.Score;
                    _context.MultiChoiceQuestions.Update(questionCopy);

                    UpdateQuestionOptions(model.Options, questionCopyId);

                    await _context.SaveChangesAsync();

                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/TextQuestion/{questionId}/Edit/", Name = "EditText")]
        public async Task<IActionResult> EditTextQuestion(int testId, int questionId,
            [FromBody] AddTextQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.TextQuestions
                .AsNoTracking()
                .SingleAsync(q => q.Id == questionId);
            if (question.TestId != testId) return NotFound();

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    // copy question
                    question.Id = 0;

                    _context.TextQuestions.Add(question);
                    await _context.SaveChangesAsync();
                    var questionCopyId = question.Id;

                    // заархивировать
                    var questionOld = await _context.TextQuestions
                        .SingleAsync(q => q.Id == questionId);
                    questionOld.IsDeleted = true;
                    _context.TextQuestions.Update(questionOld);
                    await _context.SaveChangesAsync();

                    //обновить опшены и копию
                    var questionCopy = await _context.TextQuestions
                        .SingleAsync(q => q.Id == questionCopyId);
                    questionCopy.TextRightAnswer = model.Options.Single().Text;
                    questionCopy.Title = model.Title;
                    questionCopy.Score = model.Score;
                    _context.TextQuestions.Update(questionCopy);


                    await _context.SaveChangesAsync();


                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/DragAndDropQuestion/{questionId}/Edit/", Name = "EditDragAndDrop")]
        public async Task<IActionResult> EditDragAndDropQuestion(int testId, int questionId,
            [FromBody] AddDragAndDropQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.DragAndDropQuestions
                .AsNoTracking()
                .SingleAsync(q => q.Id == questionId);
            if (question.TestId != test.Id) return NotFound();

            model.TestId = test.Id;
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    // copy question
                    question.Id = 0;

                    _context.DragAndDropQuestions.Add(question);
                    await _context.SaveChangesAsync();
                    var questionCopyId = question.Id;

                    // заархивировать
                    var questionOld = await _context.DragAndDropQuestions
                        .SingleAsync(q => q.Id == questionId);
                    questionOld.IsDeleted = true;
                    _context.DragAndDropQuestions.Update(questionOld);
                    await _context.SaveChangesAsync();

                    //обновить опшены и копию
                    var questionCopy = await _context.DragAndDropQuestions
                        .Include(q => q.Options)
                        .SingleAsync(q => q.Id == questionCopyId);
                    questionCopy.Title = model.Title;
                    questionCopy.Score = model.Score;
                    _context.DragAndDropQuestions.Update(questionCopy);

                    UpdateDragAndDropQuestionOptions(model.Options, questionCopyId);

                    await _context.SaveChangesAsync();

                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);

            Response.StatusCode = StatusCodes.Status400BadRequest;
            return new JsonResult(errors);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("/Tests/{testId}/CodeQuestion/{questionId}/Edit/", Name = "EditCode")]
        public async Task<IActionResult> EditCodeQuestion(int testId, int questionId,
            [FromBody] AddCodeQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test == null) return NotFound();
            if (test.CreatedBy != user) return Forbid();
            var question = await _context.CodeQuestions
                .Include(q => q.Code)
                .SingleAsync(q => q.Id == questionId);
            if (question.TestId != test.Id) return NotFound();

            model.TestId = test.Id;
            TryValidateModel(model);
            try
            {
                _context.Remove(await _context.Codes.SingleAsync(c => c.Test == test));
            }
            catch
            {
            }
            if (ModelState.IsValid)
            {
                // транзакция
                using (var ts = _context.Database.BeginTransaction())
                {
                    var code = await _context.Codes.SingleAsync(c => c.Question == question);
                    code.Args = model.Code.Args;
                    code.Value = model.Code.Value;
                    code.Output = Compile(code);
                    var option = await _context.Options.SingleAsync(o => o.Question == question);
                    question.Code = code;
                    option.Text = model.Code.Output;
                    _context.Codes.Update(code);
                    _context.Options.Update(option);
                    _context.Questions.Update(question);
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }

                var redirectUrl = Url.Action("Details", "Test", new { id = test.Id });
                return new JsonResult(redirectUrl);
            }

            var errors = new List<ModelError>();
            foreach (var modelState in ViewData.ModelState.Values)
                foreach (var error in modelState.Errors)
                    errors.Add(error);

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
            question.IsDeleted = true;
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Test", new { id = testId });
        }

        #endregion

        #region Code

        [Authorize]
        [HttpGet]
        [Route("/Tests/{testId}/Code/", Name = "GetCode")]
        public async Task<IActionResult> GetCode(int testId)
        {
            Code code;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test.CreatedById != user.Id) return BadRequest();
            try
            {
                code = await _context.Codes.SingleAsync(c => c.Test == test);
            }
            catch (Exception)
            {

                code = new Code { Output = "Output", Test = test };
                code = (await _context.AddAsync(code)).Entity;
                await _context.SaveChangesAsync();
            }

            return PartialView("CodeOutput", code);
        }

        [Authorize]
        [HttpPost]
        [Route("/Tests/{testId}/Code/", Name = "PostCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCode(int testId, [FromBody] Code model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            if (test.CreatedById != user.Id) return BadRequest();
            Code code = await _context.Codes.SingleOrDefaultAsync(c => c.Test == test);
            if (code == null)
            {
                code = (await _context.AddAsync(new Code { Test = test })).Entity;
                await _context.SaveChangesAsync();
            }
            if (test.CreatedBy != user) return BadRequest();
            code.Value = model.Value;
            code.Args = model.Args;
            code.Output = Compile(code);

            _context.Codes.Update(code);

            await _context.SaveChangesAsync();
            return new JsonResult("");
        }
        #endregion

        #region Вспомогательные методы

        private async void UpdateQuestionOptions(List<OptionViewModel> options, int questionId)
        {
            foreach (var option in options)
                await _context.Options.AddAsync(new Option
                {
                    IsRight = option.IsRight,
                    QuestionId = questionId,
                    Text = option.Text
                });
            await _context.SaveChangesAsync();
        }

        private async void UpdateDragAndDropQuestionOptions(List<OptionViewModel> options, int questionId)
        {
            foreach (var option in options)
                await _context.Options.AddAsync(new Option
                {
                    IsRight = option.IsRight,
                    QuestionId = questionId,
                    Text = option.Text,
                    Order = option.Order
                });
            await _context.SaveChangesAsync();
        }

        public static string Compile(Code code)
        {
            var TimeoutSeconds = 5;
            var output = new StringBuilder();
            object[] args;
            var syntaxTree = CSharpSyntaxTree.ParseText(code.Value);
            var assemblyName = Path.GetRandomFileName();
            MetadataReference[] references =
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    true,
                    optimizationLevel: OptimizationLevel.Release,
                    generalDiagnosticOption: ReportDiagnostic.Error,
                    warningLevel: 0));
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                        output.AppendFormat("{0}: {1}\n", diagnostic.Id, diagnostic.GetMessage());
                }
                else
                {
                    try
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        var assembly = Assembly.Load(ms.ToArray());
                        var type = assembly.GetType("TestsApp.Program");
                        var obj = Activator.CreateInstance(type);
                        var method = type.GetMethod("Main");
                        var parameters = method.GetParameters();
                        var types = new List<Type>();
                        foreach (var p in parameters) types.Add(p.ParameterType);
                        var multiArgs = code.Args.Split(';').Select(arg => arg.Trim()).ToArray();
                        string[] tmp;
                        foreach (var a in multiArgs)
                        {
                            tmp = a.Split(',').Select(arg => arg.Trim()).ToArray();
                            if (!string.IsNullOrEmpty(a))
                            {
                                args = new object[tmp.Length];
                                for (var i = 0; i < tmp.Length; i++) args[i] = Convert.ChangeType(tmp[i], types[i]);
                            }
                            else
                            {
                                args = null;
                            }

                            var task = Task.Run(() => type.InvokeMember("Main",
                                BindingFlags.Default | BindingFlags.InvokeMethod,
                                null,
                                obj,
                                args));
                            task.Wait(TimeSpan.FromSeconds(TimeoutSeconds));
                            if (task.IsCompleted)
                                output.AppendLine(task.Result.ToString());
                            else
                                throw new TimeoutException("Timed out 5sec");
                        }
                    }
                    catch (TimeoutException)
                    {
                        output = new StringBuilder($"TimeoutException: max {TimeoutSeconds * 1000} ms.");
                    }
                    catch (Exception e)
                    {
                        output = new StringBuilder(e.Message);
                    }
                }
            }

            return output.ToString();
        }

        #endregion
    }
}