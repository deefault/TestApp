using System;
using System.Collections.Generic;
using System.Text;
using WebApplication3.Models;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;


namespace WebApplication3.Data
{
    class DbInitializer
    {
        private const string PASSWORD = "Qwerty123";
        
        private readonly UserManager<User> _userManager;
        //private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly SignInManager<User> _signInManager;
        public DbInitializer(ApplicationDbContext context, 
            UserManager<User> userManager,SignInManager<User> signInManager,
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
            var users = new User[]
            {
                new User{UserName = "user1@example.com",Email = "user1@example.com"},
                new User{UserName = "user2@example.com",Email = "user2@example.com"},
                
            };
            
            foreach (var user in users)
            {
                var result  = await _userManager.CreateAsync(user, "Qwerty123");
                if (result.Succeeded)
                {
                    user.LockoutEnabled = true;
                    user.EmailConfirmed = false;
                    user.TwoFactorEnabled = false;
                    _logger.LogInformation(3, "User created a new account with password.");
                    
                }
                else _logger.LogCritical("Error creating User instance");
            }
            
            _context.SaveChanges();
        }
        
        private async void InitializeTests() 
        {
            var users = _context.Users.ToList();
            var test1 = new Test {CreatedBy = users[0],Name = "Test1",IsEnabled = true};
            _context.Tests.Add(test1);
            
            
            await _context.SaveChangesAsync();
        }
    }
}
