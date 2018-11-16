using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebApplication3.Models.QuestionViewModels
{
    
    public class OptionViewModel
    {
        [Required]
        public string Text { get; set; }
        [Required]
        public bool IsRight { get; set; }
    }
    
    public class AddSingleChoiceQuestionViewModel
    {
        public string Text { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int TestId { get; set; }

        [Required]
        public List<OptionViewModel> Options { get; set; }


    }
}
