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
using WebApplication3.Models.AnswerViewModels;

namespace WebApplication3.Controllers
{
    public class AnswerController : Controller
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
        public AnswerController(

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

        #region Входная точка
        [Authorize]
        [HttpGet]
        [Route("/{testResultId}/Question/{answerOrder}/")]
        public async Task<IActionResult> Answer(int testResultId, ushort answerOrder)
        {
            var testResult = await _context.TestResults
                .Include(tr => tr.Answers)
            .SingleAsync(tr => tr.Id == testResultId);
            if (testResult == null) return NotFound();
            var answers = testResult.Answers.OrderBy(a => a.Order).ToList();

            return View("Answer", answers);
        }
        #endregion

        #region GET
        [Authorize]
        [HttpGet]
        [Route("/SingleChoiceAnswer/{answerId}")]
        public async Task<IActionResult> LoadSingleChoiceAnswer(int answerId)
        {
            var answer = await _context.SingleChoiceAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Options)
                .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            return PartialView("_LoadSingleChoiceAnswer", answer);
        }

        [Authorize]
        [HttpGet]
        [Route("/TextAnswer/{answerId}")]
        public async Task<IActionResult> LoadTextAnswer(int answerId)
        {
            var answer = await _context.TextAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            return PartialView("_LoadTextAnswer", answer);
        }

        [Authorize]
        [HttpGet]
        [Route("/MultiChoiceAnswer/{answerId}")]
        public async Task<IActionResult> LoadMultiChoiceAnswer(int answerId)
        {
            var answer = await _context.MultiChoiceAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.AnswerOptions)
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Options)
                .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            var checkedOptionsIds = new List<int>();
            if (answer.AnswerOptions != null)
            {
                foreach (var answerOption in answer.AnswerOptions)
                {
                    if (answerOption.Checked) checkedOptionsIds.Add(answerOption.OptionId);
                }
            };
            ViewBag.checkedOptionsIds = checkedOptionsIds;
            return PartialView("_LoadMultiChoiceAnswer", answer);
        }

        [Authorize]
        [HttpGet]
        [Route("/DragAndDropAnswer/{answerId}")]
        public async Task<IActionResult> LoadDragAndDropAnswer(int answerId)
        {
            var answer = await _context.DragAndDropAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.DragAndDropAnswerOptions)
                    .Include(a => a.Question)
                        .ThenInclude(q => q.Options)
                .SingleAsync(a => a.Id == answerId)
                ;
            Shuffle(answer.Question.Options);
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }
            return PartialView("_LoadDragAndDropAnswer", answer);
        }
        #endregion

        #region POST
        [Authorize]
        [HttpPost]
        [Route("/SingleChoiceAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SingleChoiceAnswer(int answerId, [FromBody]SingleChoiceAnswerViewModel model)
        {

            var answer = await _context.SingleChoiceAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                    .SingleAsync(a => a.Id == answerId)
                ;
            if (model.OptionId == null)return new JsonResult("");
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            var option = await _context.Options.SingleAsync(o => o.Id == model.OptionId);
            // проверить что опшн принадлежит к вопросу
            if (!answer.Question.Options.Contains(option))
            {
                return BadRequest();
            }
            

            answer.Option = option;
            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }

        [Authorize]
        [HttpPost]
        [Route("/MultiChoiceAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultiChoiceAnswer(int answerId, [FromBody]MultiChoiceAnswerViewModel model)
        {

            var answer = await _context.MultiChoiceAnswers
                    .Include(a => a.TestResult).Include(a=>a.AnswerOptions)
                    .Include(a => a.Question).ThenInclude(a=>a.Options)
                    .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            //TODO ошибка
            // проверить что опшнsы принадлежит к вопросу
            foreach (var id in model.CheckedOptionIds)
            {
                if (!answer.Question.Options.Exists(o=>o.Id==id))
                {
                    return BadRequest();
                }
            }
            //создать
            if (answer.AnswerOptions == null)
            {
                foreach (var option in answer.Question.Options )
                {
                    _context.AnswerOptions.Add(new AnswerOption
                    {
                        Answer = answer,
                        AnswerId = answerId,
                        Option = option,
                        OptionId = option.Id,
                        Checked = model.CheckedOptionIds.Contains(option.Id)
                    });
                } 
                await _context.SaveChangesAsync();
            }
            // обновить
            else
            {
                foreach (var answerOption in answer.AnswerOptions )
                {
                    answerOption.Checked = model.CheckedOptionIds.Contains(answerOption.OptionId);
                    _context.AnswerOptions.Update(answerOption);
                }
                await _context.SaveChangesAsync();
            }
            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }
        
        [Authorize]
        [HttpPost]
        [Route("/TextAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TextAnswer(int answerId, [FromBody]TextAnswerViewModel model)
        {

            var answer = await _context.TextAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                    .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }

            answer.Text = model.Text;
            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }

        [Authorize]
        [HttpPost]
        [Route("/DragAndDropAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DragAndDropAnswer(int answerId, [FromBody]DragAndDropAnswerViewModel model)
        {

            var answer = await _context.DragAndDropAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                    .Include(a => a.DragAndDropAnswerOptions)
                    .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }
            foreach (var option in model.Options)
            {
                var optionQ = await _context.Options.SingleAsync(o => o.Id == option.OptionId);
                // проверить что опшн принадлежит к вопросу
                if (!answer.Question.Options.Contains(optionQ))
                {
                    return BadRequest();
                }
            }
            if (answer.DragAndDropAnswerOptions.Count == 0)
            {
                int i = 0;
                foreach (var option in model.Options)
                {
                    var rightOrder = answer.Question.Options.OrderBy(o => o.Order).ToList();
                    var rightOption = rightOrder[i++];
                    _context.DragAndDropAnswerOptions.Add(new DragAndDropAnswerOption
                    {
                        Answer = answer,
                        AnswerId = answerId,
                        RightOption = rightOption,
                        OptionId = option.OptionId,
                        RightOptionId = rightOption.Id,
                        ChosenOrder = option.ChosenOrder
                    });
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var rightOrder = answer.Question.Options.OrderBy(o => o.Order).ToList();
                foreach (var option in answer.DragAndDropAnswerOptions)
                {
                    option.ChosenOrder = model.Options.Single(o => o.OptionId == option.OptionId).ChosenOrder;
                    option.RightOption = rightOrder[option.ChosenOrder - 1];
                    option.RightOptionId = option.RightOption.Id;
                }
            }
            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }
        #endregion

        #region Вспомогательные методы
        private static Random rng = new Random();

        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        #endregion

        
    }
}