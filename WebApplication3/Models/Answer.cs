using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace WebApplication3.Models
{
    public abstract class Answer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Question Question;
        [Required]
        public string AnswerType { get; set; }
        [Required]
        [Range(0,0.99)]
        public float Score { get; set; }
    }
    
    public  class SingleChoiceAnswer : Answer
    {
        [Required]
        public Option Option { get; set; }
    }
    
    public  class MultiChoiceAnswer : Answer
    {
        
        public List<Option> Options { get; set; }
    }
    
    public  class TextAnswer : Answer
    {
        public string Text { get; set; }
    }
}