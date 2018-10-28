using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace WebApplication3.Models
{
    public class User  : IdentityUser
    {
        
        public ICollection<TestResult> TestResults { get; set; } //Completed tests
        public ICollection<Test> Tests { get; set; } // uncompleted tests
    }
}