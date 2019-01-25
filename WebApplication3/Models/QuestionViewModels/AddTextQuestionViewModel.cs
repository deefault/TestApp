using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddTextQuestionViewModel : QuestionViewModel
    {
        public class TextViewModel
        {
            [Required]
            public string Text { get; set; }
        }
        [Required]
        public string QuestionType { get; set; }
        
        [OptionsValidation]
        public List<TextViewModel> Options { get; set; }

        public class OptionsValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var options = value as List<TextViewModel>;
                return ValidationResult.Success;
            }
        }
    }
}
