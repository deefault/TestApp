using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace WebApplication3.Models.QuestionViewModels
{
    public class AddMultiChoiceQuestionViewModel
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