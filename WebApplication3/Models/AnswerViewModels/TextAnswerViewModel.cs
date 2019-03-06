using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.AnswerViewModels
{
    public class TextAnswerViewModel
    {
        [Required]
        public string Text { get; set; }
    }
}
