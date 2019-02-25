using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.AnswerViewModels
{
    public class DragAndDropAnswerOptionViewModel
    {
        [Required]
        public int? Id { get; set; } = null;
        [Required]
        public int ChosenOrder { get; set; }
        [Required]
        public int OptionId { get; set; }
    }
    public class DragAndDropAnswerViewModel
    {
        [Required]
        public List<DragAndDropAnswerOptionViewModel> Options { get; set; }
    }
}