using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WebApplication3.Models.TestViewModels
{
    public class AddTestViewModel
    {
        [Required]
        [DisplayName("Имя")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Состояние	")]
        public bool IsEnabled { get; set; }
    }
}