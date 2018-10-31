﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication3.Data;
using WebApplication3.Models;
using Microsoft.Extensions.DependencyInjection;

//test
namespace WebApplication3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            
            
            var host = CreateWebHostBuilder(args).Build();
            
            
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    //DbInitializer.Initialize(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            host.Run();
            
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            string contentRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
            

    }
}