using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;


namespace WebApplication3.Models
{
    public class User  : IdentityUser<int>
    {
        
        public ICollection<TestResult> TestResults { get; set; }
        public ICollection<Test> Tests { get; set; }
        public User() : base()
        {  
            TestResults = new List<TestResult>();
            Tests = new List<Test>();
        }
    }
}