using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddCodeQuestionViewModel : QuestionViewModel
    {
        [Required]
        [OptionsValidation]
        public List<OptionViewModel> Options { get; set; }
        //[Required]
        //[DisplayName("Код")]
        //public bool Code { get; set; }

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
