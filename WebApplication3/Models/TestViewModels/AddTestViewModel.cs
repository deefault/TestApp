using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace WebApplication3.Models.TestViewModels
{
    public class AddTestViewModel
    {
        [Required]
        [DisplayName("Название теста")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Включить")]
        public bool IsEnabled { get; set; }
        
    }
}