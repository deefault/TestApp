using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace WebApplication3.Models.TestViewModels
{
    public class AddTestToUserViewModel
    {
        //public User User { get; set; }
        [Required]
        public Test Test { get; set; }
        
    }
}