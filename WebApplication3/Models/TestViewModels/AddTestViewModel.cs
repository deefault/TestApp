using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace WebApplication3.Models.TestViewModels
{
    public class AddTestViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsEnabled { get; set; }
        
    }
}