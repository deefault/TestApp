using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddDragAndDropQuestionViewModel : QuestionViewModel
    {
        [Required]
        [OptionsValidation]
        public List<OptionViewModel> Options { get; set; }
        public int Order { get; set; }

        public class OptionsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var options = value as List<OptionViewModel>;
                return ValidationResult.Success;
            }
        }
    }
}
