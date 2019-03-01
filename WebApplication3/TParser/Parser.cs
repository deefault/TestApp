using System.Collections;
using System.Collections.Generic;
using System;
using WebApplication3.Models;

namespace WebApplication3.TParser
{
    class Parser
    {
        private static Hashtable _types;
        public static TestData Parse(Queue<string> tokens)
        {
            _types = Hashtable.Synchronized(new Hashtable());
            _types["SingleChoiceQuestion"] = 1;
            _types["MultiChoiceQuestion"] = 2;
            _types["TextQuestion"] = 3;
            _types["DragAndDropQuestion"] = 4;
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
            int? token = (int?)_types[tokens.Peek()];
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
        private static TestData ParseTest(Queue<string> tokens)
        {
            TestData testData = new TestData();
            var test = new Test();
            Consume(tokens, "test");
            Consume(tokens, "{");
            test.Name = ParseText(tokens);
            test.IsEnabled = ParseFlag(tokens);
            while (tokens.Peek() == "question")
            {
                ParseQuestion(tokens, test, testData);
            }
            Consume(tokens, "}");
            testData.Test = test;
            return testData;
        }
        private static void ParseQuestion(Queue<string> tokens, Test test, TestData testData)
        {
            Question question;
            Consume(tokens, "question");
            Consume(tokens, "{");
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
            question.Title = ParseText(tokens);
            question.QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), type);
            while (tokens.Peek() == "option")
            {
                ParseOption(tokens, question, testData);
            }
            Consume(tokens, "}");
            testData.Questions.Add(question);
        }
        private static void ParseOption(Queue<string> tokens, Question question, TestData testData)
        {
            Option option = new Option { Question = question };
            Consume(tokens, "option");
            Consume(tokens, "{");
            option.Text = ParseText(tokens);
            option.IsRight = ParseFlag(tokens);
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
        private static int ParseType(Queue<string> tokens)
        {
            Consume(tokens, "type");
            Consume(tokens, "=");
            var type = ConsumeType(tokens);
            return type;
        }
    }
}
