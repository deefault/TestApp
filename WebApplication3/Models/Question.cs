using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace WebApplication3.Models
{
    
    
    public abstract class Question
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        
        [Required]
        public Test Test { get; set; }
        
        [Required]
        public string QuestionType { get; set; }
        
        public List<Option> Options { get; set; }
    }

    public class SingleChoiceQuestion : Question
    {
               
        public Option RightAnswer { get; set; }

    }
    
    public class MultiChoiceQuestion : Question
    {
        
    }
    
    public class TextQuestion : Question
    {
        [Required]
        public string TextRightAnswer { get; set; }
    }

    public class Option
    {
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public bool IsRight { get; set; }
        [Required]
        public Question Question { get; set; }
        
    }
    
}