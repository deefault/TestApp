using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    internal class DbInitializer
    {
        private const string PASSWORD = "Qwerty123";

        private static readonly Random _random = new Random();

        //private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public DbInitializer(ApplicationDbContext context,
            UserManager<User> userManager, SignInManager<User> signInManager,
            ILogger logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        // добавляет в текущую базу
        public async void Initialize()
        {
            _context.Database.EnsureCreated();

            await _context.SaveChangesAsync();
        }

        // удаляет базу и создает новую
        public void InitializeNew()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            InitializeUsers();
            //InitializeTests();
        }

        private async void InitializeUsers()
        {
            var users = new[]
            {
                new User {UserName = "user1@example.com", Email = "user1@example.com"},
                new User {UserName = "user2@example.com", Email = "user2@example.com"}
            };

            foreach (var user in users)
            {
                var result = await _userManager.CreateAsync(user, "Qwerty123");
                if (result.Succeeded)
                {
                    user.LockoutEnabled = true;
                    user.EmailConfirmed = false;
                    user.TwoFactorEnabled = false;
                    _logger.LogInformation(3, "User created a new account with password.");
                }
                else
                {
                    _logger.LogCritical("Error creating User instance");
                }
            }

            _context.SaveChanges();
        }

        private async void InitializeTests()
        {
            var users = _context.Users.ToList();
            var test1 = new Test {CreatedBy = users[0], Name = "Test1", IsEnabled = true};
            _context.Tests.Add(test1);


            await _context.SaveChangesAsync();
        }

        private async Task<User> GenerateUser()
        {
            var count = _context.Users.Count();
            var user = new User {UserName = $"user{count}@example.com", Email = $"user{count}@example.com"};
            if (await _userManager.FindByNameAsync(user.UserName) != null ||
                await _userManager.FindByEmailAsync(user.Email) != null)
            {
                var name = GenerateExampleEmail();
                user.UserName = name;
                user.Email = name;
            }

            var result = await _userManager.CreateAsync(user, "Qwerty123");
            if (result.Succeeded)
            {
                user.LockoutEnabled = true;
                user.EmailConfirmed = false;
                user.TwoFactorEnabled = false;
                _logger.LogInformation(3, "User created a new account with password.");
            }
            else
            {
                _logger.LogCritical("Error creating User instance");
            }

            _context.SaveChanges();
            return user;
        }

        public async void GetStatsForTest(int testId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                CompleteTest(testId);
            }
        }
        private async void CompleteTest(int testId)
        {
            var user = await GenerateUser();
            var test = _context.Tests.Include(t => t.Questions).SingleOrDefault(t => t.Id == testId);
            // add test to user
            var testResult = new TestResult
            {
                CompletedByUser = user,
                IsCompleted = false,
                Test = test
            };
            await _context.TestResults.AddAsync(testResult);
            // start test
            await StartTest(user, testResult, test);

            await _context.Entry(testResult).Collection(x=>x.Answers).LoadAsync();
            foreach (var answer in testResult.Answers)
            {
                switch (answer.AnswerType)
                {
                    case "SingleChoiceAnswer": AnswerSingleChoice(answer);
                        break;
                    case "MultiChoiceAnswer": AnswerMultiChoice(answer);
                        break;
                    case "TextAnswer": AnswerText(answer);
                        break;
                    case "DragAndDropAnswer": AnswerDragAndDrop(answer);
                        break;
                    case "CodeAnswer": AnswerCode(answer);
                        break;
                }
            }

            _context.SaveChanges();
        }

        private int GetRandomOptionId(List<Option> options)
        {
            return options[_random.Next(0, options.Count)].Id;
        }
        
        private int [] GetRandomOptionIdArray(List<Option> options)
        {
            int [] optionIds = options.Select(c => c.Id).ToArray();
            var n = options.Count(o => o.IsRight);
            var result = new int[n]; 
            for (int i=0;i<n;i++)
            {
                int randomIdIndex;
                do
                {
                    randomIdIndex= _random.Next(0, optionIds.Length);
                    result[i] = optionIds[randomIdIndex];
                } while (result[i] == 0);
                optionIds[randomIdIndex] = 0;
            }

            return result;
        }
        private void AnswerSingleChoice(Answer _answer)
        {
            var answer = _answer as SingleChoiceAnswer;
            _context.Entry(answer).Reference(x=>x.Question).Load();
            _context.Entry(answer.Question).Collection(x=>x.Options).Load();
            answer.OptionId = GetRandomOptionId(answer.Question.Options);
            _context.Update(answer);
        }

        private void AnswerMultiChoice(Answer _answer)
        {
            var answer = _answer as MultiChoiceAnswer;
            _context.Entry(answer).Reference(x=>x.Question).Load();
            _context.Entry(answer.Question).Collection(x=>x.Options).Load();
            var checkedIds = GetRandomOptionIdArray(answer.Question.Options);
            foreach (var option in answer.Question.Options)
            {
                var answerOption = new AnswerOption
                {
                    AnswerId = answer.Id,
                    Checked = checkedIds.Contains(option.Id),
                    Option = option
                };
                _context.Add(answerOption);
            }
            _context.Update(answer);
        }

        private void AnswerText(Answer _answer)
        {
            var answer = _answer as TextAnswer;
            throw new NotImplementedException();
        }

        private void AnswerDragAndDrop(Answer _answer)
        {
            var answer = _answer as DragAndDropAnswer;
            throw new NotImplementedException();
        }

        private void AnswerCode(Answer _answer)
        {
            var answer = _answer as CodeAnswer;
            throw new NotImplementedException();
        }

        private async Task StartTest(User user, TestResult testResult, Test test)
        {

            var answers = new List<Answer>();
            Answer answer = null;
            var questions = testResult.Test.Questions.Where(q => !q.IsDeleted).ToList();
            if (test.Count != 0 && test.Count < test.Questions.Count && testResult.Test.Shuffled)
            {
                
                var order = new ushort[testResult.Test.Count];
                for (var i = 0; i < order.Length; i++)
                    order[i] = (ushort) (i + 1);

                var j = 0;
                for (var k = 0; k < order.Length; k++)
                {
                    var question = questions[k];
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
                        case "CodeQuestion":
                            answer = new CodeAnswer();
                            break;
                    }

                    if (answer == null) throw new NullReferenceException();
                    answer.Result = null;
                    answer.Question = question;
                    answer.Score = 0;
                    answer.TestResult = testResult;
                    answer.Order = order[j++];
                    await _context.Answers.AddAsync(answer);
                    answers.Add(answer);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var order = new ushort[questions.Count()];
                for (var i = 0; i < order.Length; i++)
                    order[i] = (ushort) (i + 1);

                var j = 0;
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
                        case "CodeQuestion":
                            answer = new CodeAnswer();
                            break;
                    }

                    if (answer == null) throw new NullReferenceException();
                    answer.Question = question;
                    answer.Score = 0;
                    answer.TestResult = testResult;
                    answer.Order = order[j++];
                    await _context.Answers.AddAsync(answer);
                    answers.Add(answer);
                    await _context.SaveChangesAsync();
                }
            }

            testResult.StartedOn = DateTime.UtcNow;
            _context.TestResults.Update(testResult);
            await _context.SaveChangesAsync();

        }

        private static string GenerateExampleEmail()
        {
            return RandomString(8) + "@example.com";
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}