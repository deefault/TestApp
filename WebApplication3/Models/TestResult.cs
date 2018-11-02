using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public bool isCompleted { get; set; }
        [Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CompletedOn { get; set; } //= DateTime.UtcNow;

        public ICollection<Answer> Answers { get; set; }
        
        
        public uint RightAnswersCount { get; set; }
        
        public uint TotalQuestions { get; set; }
        
        [Required]
        public User  CompletedByUser { get; set; }
        [Required]
        public Test Test { get; set; }
       
    }
}