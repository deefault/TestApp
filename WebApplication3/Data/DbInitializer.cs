using System;
using System.Collections.Generic;
using System.Text;
using WebApplication3.Models;
using System.Linq;

namespace WebApplication3.Data
{
    class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

           
            if (context.Users.Any())
            {
                return;
            }

            var users = new User[]
            {
                new User{UserName = "user1",Email = "example@sasd.com"},
                new User{UserName = "user2",Email = "example2@sasd.com"},
                
            };
            
            foreach (var user in users)
            {
                context.Users.Add(user);
            }

            context.SaveChanges();

        }
    }
}
