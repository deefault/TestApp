using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.AnswerViewModels
{
    public class SingleChoiceAnswerViewModel
    {
        [Required]
        public int OptionId { get; set; }
    }
}