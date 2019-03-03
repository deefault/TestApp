using System.Collections;
using System.Collections.Generic;
using System;
using WebApplication3.Models;
using System.Linq;

namespace WebApplication3.TParser
{
    class Parser
    {
        private static Hashtable _types;
        public static TestData Parse(Queue<string> tokens)
        {
            _types = Hashtable.Synchronized(new Hashtable());
            _types["single"] = 1;
            _types["multi"] = 2;
            _types["text"] = 3;
            _types["dnd"] = 4;
            try
            {
                TestData testData = ParseTest(tokens);
                return testData;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        private static string Consume(Queue<string> tokens, string desired)
        {
            string token = tokens.Peek().ToLower();
            if (token == desired)
            {
                return tokens.Dequeue();
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}", desired, token));

        }
        private static int ConsumeType(Queue<string> tokens)
        {
            int? token = (int?)_types[tokens.Peek().ToLower()];
            if (token != null)
            {
                tokens.Dequeue();
                return (int)token;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}", "QuestionType", tokens.Peek()));

        }
        private static bool ConsumeFlag(Queue<string> tokens)
        {
            bool flag = Boolean.TryParse(tokens.Peek(), out bool tmp);
            if (flag)
            {
                tokens.Dequeue();
                return tmp;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}", "Boolean", tokens.Peek()));
        }
        private static int ConsumeScore(Queue<string> tokens)
        {
            bool flag = Int32.TryParse(tokens.Peek(), out int tmp);
            if (flag)
            {
                tokens.Dequeue();
                return tmp;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}", "Int32", tokens.Peek()));
        }
        private static TestData ParseTest(Queue<string> tokens)
        {
            TestData testData = new TestData();
            var test = new Test();
            bool textParsed = false, flagParsed = false;
            Consume(tokens, "test");
            Consume(tokens, "{");
            while (tokens.Peek() != "}") 
                switch (tokens.Peek())
                {
                    case ("text"):
                        if (!textParsed)
                            test.Name = ParseText(tokens);
                        else
                            throw new Exception("Text already parsed");
                        textParsed = true;
                        break;
                    case ("flag"):
                        if (!flagParsed)
                            test.IsEnabled = ParseFlag(tokens);
                        else
                            throw new Exception("Flag already parsed");
                        flagParsed = true;
                        break;
                    case ("question"):
                        ParseQuestion(tokens, test, testData);
                        break;
                }
            //test.Name = ParseText(tokens);
            //test.IsEnabled = ParseFlag(tokens);
            //while (tokens.Peek() == "question")
            //{
            //    ParseQuestion(tokens, test, testData);
            //}
            Consume(tokens, "}");
            testData.Test = test;
            if (!textParsed || !flagParsed)
                throw new Exception("Заданы не все требуемые поля (Test)");
            return testData;
        }
        private static void ParseQuestion(Queue<string> tokens, Test test, TestData testData)
        {
            Question question;
            bool textParsed = false, scoreParsed = false, optionParsed = false;
            Consume(tokens, "question");
            Consume(tokens, "{");
            int i = 1, checkedCount = 0;
            var type = ParseType(tokens);
            switch (type)
            {
                case (int)Question.QuestionTypeEnum.SingleChoiceQuestion:
                    question = new SingleChoiceQuestion();
                    break;
                case (int)Question.QuestionTypeEnum.MultiChoiceQuestion:
                    question = new MultiChoiceQuestion();
                    break;
                case (int)Question.QuestionTypeEnum.TextQuestion:
                    question = new TextQuestion();
                    break;
                case (int)Question.QuestionTypeEnum.DragAndDropQuestion:
                    question = new DragAndDropQuestion();
                    break;
                default:
                    question = new SingleChoiceQuestion();
                    break;
            }
            question.Test = test;
            while (tokens.Peek() != "}")
                switch (tokens.Peek())
                {
                    case ("text"):
                        if (!textParsed)
                            question.Title = ParseText(tokens);
                        else
                            throw new Exception("Text already parsed");
                        textParsed = true;
                        break;
                    case ("score"):
                        if (!scoreParsed)
                            question.Score = ParseScore(tokens);
                        else
                            throw new Exception("Score already parsed");
                        scoreParsed = true;
                        break;
                    case ("option"):
                        if (question is TextQuestion)
                            if (i == 1)
                                ParseOption(tokens, question, testData, i++, ref checkedCount);
                            else
                                throw new Exception("TextQuestion поддерживает только один опшн");
                        else
                            ParseOption(tokens, question, testData, i++, ref checkedCount);
                        optionParsed = true;
                        break;
                }
            question.QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), type);
            //if (question is TextQuestion)
            //    ParseOption(tokens, question, testData, i++, ref checkedCount);
            //else
            //{
            //    while (tokens.Peek() == "option")
            //    {
            //        ParseOption(tokens, question, testData, i++, ref checkedCount);
            //    }
            //    if (question is SingleChoiceQuestion && checkedCount != 1)
            //    {
            //            throw new Exception("В данном типе вопроса нужно отметить один ответ.");
            //    }
            //    else if (question is MultiChoiceQuestion && checkedCount < 1)
            //        throw new Exception("В данном типе вопроса нужно отметить хотя бы один ответ.");
            //}
            if (question is SingleChoiceQuestion && checkedCount != 1)
            {
                throw new Exception("В данном типе вопроса нужно отметить один ответ.");
            }
            else if (question is MultiChoiceQuestion && checkedCount < 1)
                throw new Exception("В данном типе вопроса нужно отметить хотя бы один ответ.");
            Consume(tokens, "}");
            if (!textParsed || !scoreParsed || !optionParsed)
                throw new Exception("Заданы не все требуемые поля (Quesiton)");
            testData.Questions.Add(question);
        }
        private static void ParseOption(Queue<string> tokens, Question question, TestData testData, int i, ref int checkedCount)
        {
            Option option = new Option { Question = question };
            Consume(tokens, "option");
            Consume(tokens, "{");
            option.Text = ParseText(tokens);
            if (question is SingleChoiceQuestion || question is MultiChoiceQuestion)
            {
                option.IsRight = ParseFlag(tokens);
                if (option.IsRight)
                    checkedCount++;
            }
            else if (question is DragAndDropQuestion)
                option.Order = i;
            else
                (question as TextQuestion).TextRightAnswer = option.Text;
            Consume(tokens, "}");
            testData.Options.Add(option);
        }
        private static string ParseText(Queue<string> tokens)
        {
            Consume(tokens, "text");
            Consume(tokens, "=");
            string text = tokens.Dequeue();
            return text;
        }
        private static bool ParseFlag(Queue<string> tokens)
        {
            Consume(tokens, "flag");
            Consume(tokens, "=");
            bool flag = ConsumeFlag(tokens);
            return flag;
        }
        private static int ParseScore(Queue<string> tokens)
        {
            Consume(tokens, "score");
            Consume(tokens, "=");
            int score = ConsumeScore(tokens);
            return score;
        }
        private static int ParseType(Queue<string> tokens)
        {
            Consume(tokens, "type");
            Consume(tokens, "=");
            var type = ConsumeType(tokens);
            return type;
        }
    }
}
