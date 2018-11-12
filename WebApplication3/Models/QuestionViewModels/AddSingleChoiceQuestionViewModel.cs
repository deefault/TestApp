using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddSingleChoiceQuestionViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public int TestId { get; set; }

        [Required]
        public string QuestionType { get; set; }
    }
}
