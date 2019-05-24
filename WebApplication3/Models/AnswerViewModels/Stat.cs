using System.Collections.Generic;
using System.Linq;

namespace WebApplication3.Models.AnswerViewModels
{
    public class Stat
    {
        public string TestName { get; set; }
        public int TestId { get; set; }
        public Question MostDifficult { get; set; }
        public Question MostEasy { get; set; }
        public List<QuestionStat> QuestionStats { get; set; } = new List<QuestionStat>();
        public int GetMostDifficultId()
        {
            QuestionStat tempMax = new QuestionStat{PartiallyRightCount = -1,QuestionId = -1,WrongCount = -1,RightCount = -1};
            foreach (var q in QuestionStats)
            {
                if (q.WrongCount >= tempMax.WrongCount)
                {
                    if (q.RightCount <= tempMax.RightCount || tempMax.RightCount < 0)
                    {
                        if (q.PartiallyRightCount <= tempMax.PartiallyRightCount || tempMax.PartiallyRightCount<0)
                        {
                            tempMax.QuestionId = q.QuestionId;
                            tempMax.RightCount = q.RightCount;
                            tempMax.WrongCount = q.WrongCount;
                            tempMax.PartiallyRightCount = q.PartiallyRightCount;
                        }
                    }
                }
            }

            return tempMax.QuestionId;
        }
        
        public int GetMostEasyId()
        {
            QuestionStat tempMax = new QuestionStat{PartiallyRightCount = -1,QuestionId = -1,WrongCount = -1,RightCount = -1};
            foreach (var q in QuestionStats)
            {
                if (q.RightCount >= tempMax.RightCount)
                {
                    if (q.WrongCount <= tempMax.WrongCount || tempMax.WrongCount < 0)
                    {
                        if (q.PartiallyRightCount >= tempMax.PartiallyRightCount)
                        {
                            tempMax.QuestionId = q.QuestionId;
                            tempMax.RightCount = q.RightCount;
                            tempMax.WrongCount = q.WrongCount;
                            tempMax.PartiallyRightCount = q.PartiallyRightCount;
                        }
                    }
                }
            }

            return tempMax.QuestionId;
        }
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
        
        public float AverageScore { get; set; }
        
        public float MaxScore { get; set; }
        
        public float ScoreStandartDerivation { get; set; }

    }
}