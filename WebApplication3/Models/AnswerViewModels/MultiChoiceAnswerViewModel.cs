using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WebApplication3.Models.AnswerViewModels
{
    public class AnswerOptionViewModel
    {
        [Required]
        public int? Id { get; set; } = null;
        [Required]
        public int OptionId { get; set; }
    }

    public class MultiChoiceAnswerViewModel
    {
        [Required]
        public List<AnswerOptionViewModel> Options { get; set; }
    }
}
