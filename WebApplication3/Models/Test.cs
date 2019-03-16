using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication3.Models.TestViewModels;

namespace WebApplication3.Models
{
    
    public class Test
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public User  CreatedBy { get; set; }
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;        
        public bool IsEnabled { get; set; }        
        public bool Shuffled { get; set; }
        public ICollection<Question> Questions { get; set; }
        
        public enum QuestionTypeEnum
        {
            [Display(Name = "С одним правильным ответом")]
            SingleChoiceQuestion=1,
            [Display(Name = "С несколькими правильными ответами")]
            MultiChoiceQuestion=2,
            [Display(Name = "С вводом текста")]
            TextQuestion=3,
            [Display(Name = "На восстановление последовательности")]
            DragAndDropQuestion = 4
        }        
    }
    public class AddTestModel
    {
        public List<Test> Model1 { get; set; }
        public AddTestViewModel Model2 { get; set; }
    }
}