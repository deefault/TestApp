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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CompletedOn { get; set; } = DateTime.UtcNow;


        public User  CompletedBy { get; set; }
        public Test Test { get; set; }
        //public List<Question> RightAnswers { get; set; }
        //public List<Question> WrongAnswers { get; set; }
        // TODO
    }
}