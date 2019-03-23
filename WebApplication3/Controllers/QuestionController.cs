using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
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
        [HttpPost]
        [Authorize]
        [Route("/Tests/{testId}/Question/Add/Code/", Name = "AddCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCodeQuestion([FromBody]AddCodeQuestionViewModel model)
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
                    await _context.AddAsync(
                        new Code { Args = model.Code.Args, Question = question, Output = model.Code.Output, Value = model.Code.Value });
                    await _context.AddAsync(
                        new Option { Text = model.Code.Output, Question = question });                 
                    try
                    {
                        _context.Remove(await _context.Codes.SingleAsync(c => c.Test == test));
                    }
                    catch { }
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
                case nameof(Question.QuestionTypeEnum.CodeQuestion):
                    CodeQuestion codeQuestion = await _context.CodeQuestions.SingleOrDefaultAsync(q => q.Id == questionId);
                    codeQuestion.Code = await _context.Codes.SingleOrDefaultAsync(c => c.Question == codeQuestion);
                    return View("EditCodeQuestion", codeQuestion);
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
        [Route("/Tests/{testId}/CodeQuestion/{questionId}/Edit/", Name = "EditCode")]
        public async Task<IActionResult> EditCodeQuestion(int testId, int questionId, [FromBody] AddCodeQuestionViewModel model)
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
            var question = await _context.CodeQuestions
                .Include(q => q.Code)
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
                    var code = await _context.Codes.SingleAsync(c => c.Question == question);
                    code.Args = model.Code.Args; code.Output = model.Code.Output; code.Value = model.Code.Value;
                    var option = await _context.Options.SingleAsync(o => o.Question == question);
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

        #region Code
        [Authorize]
        [HttpGet]
        [Route("/Tests/{testId}/Code/", Name = "GetCode")]
        public async Task<IActionResult> GetCode(int testId)
        {
            Code code;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            try
            {
                code = await _context.Codes.SingleAsync(c => c.Test == test);
            }
            catch (Exception)
            {
                using (var ts = _context.Database.BeginTransaction())
                {
                    code = new Code { Output = "Output", Test = test };
                    code = (await _context.AddAsync(code)).Entity;
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }
            }
            return PartialView("CodeOutput", code);
        }
        [Authorize]
        [HttpPost]
        [Route("/Tests/{testId}/Code/", Name = "PostCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCode(int testId, [FromBody]Code model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            Code code;
            try
            {
                code = await _context.Codes.SingleAsync(c => c.Test == test);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            code.Value = model.Value;
            code.Args = model.Args;
            object[] args;
            
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(model.Value);
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            StringBuilder message = new StringBuilder();
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        message.AppendFormat("{0}: {1}\n", diagnostic.Id, diagnostic.GetMessage());
                        code.Output = message.ToString();
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    Type type = assembly.GetType("TestsApp.Program");
                    object obj = Activator.CreateInstance(type);
                    if (!string.IsNullOrEmpty(model.Args))
                    {
                        var method = type.GetMethod("Main");
                        var parameters = method.GetParameters();
                        args = new object[parameters.Length];
                        List<Type> types = new List<Type>();
                        foreach (var p in parameters)
                        {
                            types.Add(p.ParameterType);
                        }
                        string[] tmp = model.Args.Split(',').Select(arg => arg.Trim()).ToArray();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            args[i] = Convert.ChangeType(tmp[i], types[i]);
                        }
                    }
                    else
                        args = null;
                    try
                    {
                        code.Output = type.InvokeMember("Main",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        obj,
                        args).ToString();
                    }
                    catch (Exception e)
                    {
                        code.Output = e.Message;
                    }
                }
            }
            using (var ts = _context.Database.BeginTransaction())
            {
                _context.Codes.Update(code);
                await _context.SaveChangesAsync();
                ts.Commit();
            }
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }
        [Authorize]
        [HttpGet]
        [Route("/Tests/{testId}/Code/{questionId}", Name = "EditGetCode")]
        public async Task<IActionResult> EditGetCode(int testId, int questionId)
        {
            Code code;
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            var question = await _context.CodeQuestions
                .SingleAsync(q => q.Id == questionId);
            try
            {
                code = await _context.Codes.SingleAsync(c => c.Question == question);
            }
            catch (Exception)
            {
                using (var ts = _context.Database.BeginTransaction())
                {
                    code = new Code { Output = "Output", Test = test };
                    code = (await _context.AddAsync(code)).Entity;
                    await _context.SaveChangesAsync();
                    ts.Commit();
                }
            }
            return PartialView("CodeOutput", code);
        }
        [Authorize]
        [HttpPost]
        [Route("/Tests/{testId}/Code/{questionId}", Name = "EditPostCode")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPostCode(int testId, int questionId, [FromBody]Code model)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var test = await _context.Tests.SingleOrDefaultAsync(t => t.Id == (int)RouteData.Values["testId"]);
            var question = await _context.CodeQuestions
                .SingleAsync(q => q.Id == questionId);
            Code code;
            try
            {
                code = await _context.Codes.SingleAsync(c => c.Question == question);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            code.Value = model.Value;
            code.Args = model.Args;
            object[] args;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(model.Value);
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            StringBuilder message = new StringBuilder();
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        message.AppendFormat("{0}: {1}\n", diagnostic.Id, diagnostic.GetMessage());
                        code.Output = message.ToString();
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    Type type = assembly.GetType("TestsApp.Program");
                    object obj = Activator.CreateInstance(type);
                    if (!string.IsNullOrEmpty(model.Args))
                    {
                        var method = type.GetMethod("Main");
                        var parameters = method.GetParameters();
                        args = new object[parameters.Length];
                        List<Type> types = new List<Type>();
                        foreach (var p in parameters)
                        {
                            types.Add(p.ParameterType);
                        }
                        string[] tmp = model.Args.Split(',').Select(arg => arg.Trim()).ToArray();
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            args[i] = Convert.ChangeType(tmp[i], types[i]);
                        }
                    }
                    else
                        args = null;
                    try
                    {
                        code.Output = type.InvokeMember("Main",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        obj,
                        args).ToString();
                    }
                    catch (Exception e)
                    {
                        code.Output = e.Message;
                    }
                }
            }
            using (var ts = _context.Database.BeginTransaction())
            {
                _context.Codes.Update(code);
                await _context.SaveChangesAsync();
                ts.Commit();
            }
            await _context.SaveChangesAsync();
            return new JsonResult("");
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