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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        private readonly SignInManager<User> _signInManager;

        //private readonly IEmailSender _emailSender;
        //private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;

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

        [Authorize]
        [HttpGet]
        [Route("/{testResultId}/Question/{answerOrder}/")]
        public async Task<IActionResult> Answer(int testResultId, ushort answerOrder)
        {
            var testResult = await _context.TestResults
                .Include(tr=> tr.Answers)
            .SingleAsync(tr=>tr.Id == testResultId);
            if (testResult == null) return NotFound();
            var answers = testResult.Answers.OrderBy(a=>a.Order).ToList();
            
            return View("Answer",answers);
        }

        [Authorize]
        [HttpGet]
        [Route("/SingleChoiceAnswer/{answerId}")]
        public async Task<IActionResult> LoadSingleChoiceAnswer(int answerId)
        {
            var answer = await _context.SingleChoiceAnswers
                    .Include(a=> a.TestResult)
                    .Include(a=>a.Question)
                        .ThenInclude(q=>q.Options)
                .SingleAsync(a=> a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (_context.TestResults.Count(tr=>tr.Id== answer.TestResult.Id && tr.CompletedByUser==user) == 0)
            {
                return NotFound();
            }

            return PartialView("_LoadSingleChoiceAnswer",answer);
        }
        
        [Authorize]
        [HttpPost]
        [Route("/SingleChoiceAnswer/{answerId}/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SingleChoiceAnswer(int answerId, [FromBody]SingleChoiceAnswerViewModel model)
        {
            
            var answer = await _context.SingleChoiceAnswers
                    .Include(a=> a.TestResult)
                    .Include(a=>a.Question)
                    .SingleAsync(a=> a.Id == answerId)
                ;
            if (answer == null) return NotFound();
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //проверить что пользоавтель может проходить тест
            if (_context.TestResults.Count(tr=>tr.Id== answer.TestResult.Id && tr.CompletedByUser==user) == 0)
            {
                return NotFound();
            }

            var option = await _context.Options.SingleAsync(o=>o.Id== model.OptionId);
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
    }
}