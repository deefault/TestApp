using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddDragAndDropQuestionViewModel
    {
        public string Text { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public int TestId { get; set; }


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
