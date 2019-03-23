using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace WebApplication3.Models
{



    public abstract class Question
    {

        public int Id { get; set; }
        [Required]
        [DisplayName("Вопрос")]
        [DataType(DataType.MultilineText)]
        public string Title { get; set; }

        [Required]
        public Test Test { get; set; }

        [Required]
        public string QuestionType { get; set; }

        public List<Option> Options { get; set; }

        [DisplayName("Балл")]
        public int Score { get; set; }

        public enum QuestionTypeEnum
        {
            [Display(Name = "С одним правильным ответом")]
            SingleChoiceQuestion = 1,
            [Display(Name = "С несколькими правильными ответами")]
            MultiChoiceQuestion = 2,
            [Display(Name = "С вводом текста")]
            TextQuestion = 3,
            [Display(Name = "На восстановление последовательности")]
            DragAndDropQuestion = 4,
            [Display(Name = "На написание кода")]
            CodeQuestion = 5
        }
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

    public class DragAndDropQuestion : Question
    {

    }

    public class CodeQuestion: Question
    {
        public Code Code { get; set; }
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
        [Required]
        public int Order { get; set; }

        public ICollection<AnswerOption> AnswerOptions { get; set; }
    }
    public class Code
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Output { get; set; }
        public Question Question { get; set; }
        public Answer Answer { get; set; }
        public User User { get; set; }
        public string Args { get; set; }
    }
}