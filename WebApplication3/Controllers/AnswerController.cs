﻿using System;
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
            return PartialView("_LoadMultiChoiceAnswer", answer);
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
        [Route("/MultiChoiceAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultiChoiceAnswer(int answerId, [FromBody]MultiChoiceAnswerViewModel model)
        {

            var answer = await _context.MultiChoiceAnswers
                    .Include(a => a.TestResult)
                    .Include(a => a.Question)
                    .Include(a => a.AnswerOptions)
                    .SingleAsync(a => a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr => tr.Id == answer.TestResult.Id && tr.CompletedByUser == user) == 0)
            {
                return NotFound();
            }
            //обновление опшнов
            var options = model.Options;
            var optionsToCreate = new List<AnswerOptionViewModel>();
            var otherOptions = new List<AnswerOptionViewModel>();
            var optionsToDelete = new List<AnswerOption>();


            foreach (var option in options)
            {
                if (option.Id == null) optionsToCreate.Add(option);
                else otherOptions.Add(option);
            }

            List<int?> optionsIds = otherOptions.Select(o => o.Id).ToList();
            
            optionsToDelete = answer.AnswerOptions.Where(o => !optionsIds.Contains(o.Id)).ToList();

            foreach (var option in optionsToDelete)
            {
                _context.AnswerOptions.Remove(option);
            }

            await _context.SaveChangesAsync();

            foreach (var option in optionsToCreate)
            {
                var optionC = new AnswerOption { OptionId = option.OptionId, Answer = answer };
                var optionQ = await _context.Options.SingleAsync(o => o.Id == option.OptionId);
                // проверить что опшн принадлежит к вопросу
                if (!answer.Question.Options.Contains(optionQ))
                {
                    return BadRequest();
                }
                _context.AnswerOptions.Add(optionC);
            }
            await _context.SaveChangesAsync();

            _context.Answers.Update(answer);
            await _context.SaveChangesAsync();
            return new JsonResult("");
        }
        #endregion
    }
}