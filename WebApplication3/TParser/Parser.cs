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
        public static TestData Parse(Queue<Token> tokens)
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
        private static string Consume(Queue<Token> tokens, string desired)
        {
            string token = tokens.Peek().Value.ToLower();
            if (token == desired)
            {
                return tokens.Dequeue().Value;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}; Row - {2}", desired, token, tokens.Peek().Row));

        }
        private static int ConsumeType(Queue<Token> tokens)
        {
            int? token = (int?)_types[tokens.Peek().Value.ToLower()];
            if (token != null)
            {
                tokens.Dequeue();
                return (int)token;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}; Row - {2}", "QuestionType", tokens.Peek().Value, tokens.Peek().Row));

        }
        private static bool ConsumeFlag(Queue<Token> tokens)
        {
            bool flag = Boolean.TryParse(tokens.Peek().Value, out bool tmp);
            if (flag)
            {
                tokens.Dequeue();
                return tmp;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}; Row - {2}", "Boolean", tokens.Peek().Value, tokens.Peek().Row));
        }
        private static int ConsumeScore(Queue<Token> tokens)
        {
            bool flag = Int32.TryParse(tokens.Peek().Value, out int tmp);
            if (flag)
            {
                tokens.Dequeue();
                return tmp;
            }
            throw new Exception(String.Format("Expected: {0}; Found: {1}; Row - {2}", "Int32", tokens.Peek().Value, tokens.Peek().Row));
        }
        private static TestData ParseTest(Queue<Token> tokens)
        {
            TestData testData = new TestData();
            var test = new Test();
            bool textParsed = false, flagParsed = false;
            int Row = tokens.Peek().Row;
            Consume(tokens, "test");
            Consume(tokens, "{");
            while (tokens.Peek().Value != "}") 
                switch (tokens.Peek().Value)
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
            //while (tokens.Peek().Value == "question")
            //{
            //    ParseQuestion(tokens, test, testData);
            //}
            Consume(tokens, "}");
            testData.Test = test;
            if (!textParsed || !flagParsed)
                throw new Exception(String.Format("Заданы не все требуемые поля (Test (Row - {0})). Text - {1}, Flag - {2}.", Row, textParsed, flagParsed));
            return testData;
        }
        private static void ParseQuestion(Queue<Token> tokens, Test test, TestData testData)
        {
            Question question;
            int i = 1, checkedCount = 0, Row = 0;
            bool textParsed = false, scoreParsed = false, optionParsed = false;
            Row = tokens.Peek().Row;
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
            while (tokens.Peek().Value != "}")
                switch (tokens.Peek().Value)
                {
                    case ("text"):
                        if (!textParsed)
                            question.Title = ParseText(tokens);
                        else
                            throw new Exception(String.Format("Text already parsed (Quesiton (Row - {0})).", Row));
                        textParsed = true;
                        break;
                    case ("score"):
                        if (!scoreParsed)
                            question.Score = ParseScore(tokens);
                        else
                            throw new Exception(String.Format("Score already parsed (Quesiton (Row - {0})).", Row));
                        scoreParsed = true;
                        break;
                    case ("option"):
                        if (question is TextQuestion)
                            if (i == 1)
                                ParseOption(tokens, question, testData, i++, ref checkedCount);
                            else
                                throw new Exception(String.Format("TextQuestion поддерживает только один опшн (Quesiton (Row - {0})).", Row));
                        else
                            ParseOption(tokens, question, testData, i++, ref checkedCount);
                        optionParsed = true;
                        break;
                }
            question.QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), type);
            if (question is SingleChoiceQuestion && checkedCount != 1)
            {
                throw new Exception(String.Format("В данном типе вопроса нужно отметить один ответ (Quesiton (Row - {0})).", Row));
            }
            else if (question is MultiChoiceQuestion && checkedCount < 1)
                throw new Exception(String.Format("В данном типе вопроса нужно отметить хотя бы один ответ (Quesiton (Row - {0})).", Row));
            Consume(tokens, "}");
            if (!textParsed || !scoreParsed || !optionParsed)
                throw new Exception(String.Format("Заданы не все требуемые поля (Quesiton (Row - {0})). Text - {1}, Score - {2}, Option - {3}.", Row, textParsed, scoreParsed, optionParsed));
            testData.Questions.Add(question);
        }
        private static void ParseOption(Queue<Token> tokens, Question question, TestData testData, int i, ref int checkedCount)
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
        private static string ParseText(Queue<Token> tokens)
        {
            Consume(tokens, "text");
            Consume(tokens, "=");
            string text = tokens.Dequeue().Value;
            return text;
        }
        private static bool ParseFlag(Queue<Token> tokens)
        {
            Consume(tokens, "flag");
            Consume(tokens, "=");
            bool flag = ConsumeFlag(tokens);
            return flag;
        }
        private static int ParseScore(Queue<Token> tokens)
        {
            Consume(tokens, "score");
            Consume(tokens, "=");
            int score = ConsumeScore(tokens);
            return score;
        }
        private static int ParseType(Queue<Token> tokens)
        {
            Consume(tokens, "type");
            Consume(tokens, "=");
            var type = ConsumeType(tokens);
            return type;
        }
    }
}
