using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddDragAndDropQuestionViewModel : QuestionViewModel
    {
        public class DragAndDropOptionViewModel
        {
            [Required]
            public string Text { get; set; }
            [Required]
            public int Order { get; set; }
        }

        [Required]
        [OptionsValidation]
        public List<DragAndDropOptionViewModel> Options { get; set; }

        public class OptionsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var options = value as List<DragAndDropOptionViewModel>;
                return ValidationResult.Success;
            }
        }
    }
}
