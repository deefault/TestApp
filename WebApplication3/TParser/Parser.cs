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
        private static string ConsumeTag(Queue<string> tokens, string desired)
        {
            string token = tokens.Peek();
            if (token == desired)
            {
                return tokens.Dequeue();
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}", desired, token));

        }
        private static int ConsumeType(Queue<string> tokens)
        {
            var token = (int)_types[tokens.Peek()];
            if (token != 0)
            {
                tokens.Dequeue();
                return token;
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
            ConsumeTag(tokens, "<TEST>");
            test.Name = ParseText(tokens);
            test.IsEnabled = ParseFlag(tokens);
            while (tokens.Peek() == "<QUESTION>")
            {
                ParseQuestion(tokens, test, testData);
            }
            ConsumeTag(tokens, "</TEST>");
            testData.Test = test;
            return testData;
        }
        private static void ParseQuestion(Queue<string> tokens, Test test, TestData testData)
        {
            Question question;
            ConsumeTag(tokens, "<QUESTION>");
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
            while (tokens.Peek() == "<OPTION>")
            {
                ParseOption(tokens, question, testData);
            }
            ConsumeTag(tokens, "</QUESTION>");
            testData.Questions.Add(question);
        }
        private static void ParseOption(Queue<string> tokens, Question question, TestData testData)
        {
            Option option = new Option { Question = question };
            ConsumeTag(tokens, "<OPTION>");
            option.Text = ParseText(tokens);
            option.IsRight = ParseFlag(tokens);
            ConsumeTag(tokens, "</OPTION>");
            testData.Options.Add(option);
        }
        private static string ParseText(Queue<string> tokens)
        {
            ConsumeTag(tokens, "<TEXT>");
            string text = tokens.Dequeue();
            ConsumeTag(tokens, "</TEXT>");
            return text;
        }
        private static bool ParseFlag(Queue<string> tokens)
        {
            ConsumeTag(tokens, "<FLAG>");
            bool flag = ConsumeFlag(tokens);
            ConsumeTag(tokens, "</FLAG>");
            return flag;
        }
        private static int ParseType(Queue<string> tokens)
        {
            ConsumeTag(tokens, "<TYPE>");
            var type = ConsumeType(tokens);
            ConsumeTag(tokens, "</TYPE>");
            return type;
        }
    }
}
