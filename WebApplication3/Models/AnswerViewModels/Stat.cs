using System.Collections.Generic;

namespace WebApplication3.Models.TestViewModels
{
    public class Stat
    {
        public string TestName { get; set; }
        public int TestId { get; set; }

        public Question MostDifficult { get; set; }

        public Question MostEasy { get; set; }

        public List<QuestionStat> QuestionStats { get; set; } = new List<QuestionStat>();
        
    }

    public class QuestionStat
    {
        public int QuestionId { get; set; }
        //public Question Question { get; set; }

        public int RightCount { get; set; }
        public int PartiallyRightCount { get; set; }
        public int WrongCount { get; set; }
        public int NullCount { get; set; }
        public string QuestionType { get; set; }
        public string QuestionTitle { get; set; }
    }
}