using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models.QuestionViewModels
{
    public class AddCodeQuestionViewModel : QuestionViewModel
    {
        public Code Code { get; set; }
    }
}
