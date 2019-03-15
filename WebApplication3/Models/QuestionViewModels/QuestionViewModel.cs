using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Models.QuestionViewModels
{
    public class QuestionViewModel
    {
        public string Text { get; set; }

        [Required]
        [DisplayName("Вопрос")]
        public string Title { get; set; }

        [Required]
        public int TestId { get; set; }

        [Range(1, 100)]
        [DisplayName("Балл")]
        public int Score { get; set; } = 1;
    }
}