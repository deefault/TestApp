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
        [DisplayName("Состояние")]
        public bool IsEnabled { get; set; }
        [Required]
        [DisplayName("Перемешивать")]
        public bool Shuffled { get; set; }
        [Required]
        [DisplayName("Ответы")]
        public bool HideRightAnswers { get; set; } = false;
        [Required]
        [DisplayName("Вопросов при прохождении")]
        public int Count { get; set; } = 0;
    }
}