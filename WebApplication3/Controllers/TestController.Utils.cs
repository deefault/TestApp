using System;
using System.Collections.Generic;
using System.IO;
using WebApplication3.Models;
using System.Text;
using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication3.Controllers
{
    public partial class TestController
    {
        #region CharStream
        public class CharStream
        {
            private static readonly int _EOF = -1;
            private static readonly int _UNDEF = -2;

            private StreamReader _reader;
            private int _buffer;

            public CharStream(StreamReader reader)
            {
                _reader = reader;
                _buffer = _UNDEF;
            }
            private void FillBuffer()
            {
                try
                {
                    if (_buffer == _UNDEF)
                    {
                        _buffer = _reader.Read();
                    }
                }
                catch (Exception e)
                {
                    _buffer = _EOF;
                    Console.Write(e.Message);
                }
            }
            public virtual bool InBounds()
            {
                FillBuffer();
                return (_buffer != _EOF);
            }
            public virtual char Peek()
            {
                FillBuffer();
                if (_buffer == _EOF)
                {
                    throw new Exception("End of the file");
                }
                return (char)_buffer;
            }
            public virtual char Dequeue()
            {
                char ch = Peek();
                _buffer = _UNDEF;
                if (ch == '\n')
                {
                    LexemLocation.RowNumber++;
                    //LexemLocation.colNumber = 1;
                }
                //else
                //{
                //LexemLocation.colNumber++;
                //}
                return ch;
            }
        }
        public class LexemLocation
        {
            public static int RowNumber { get; set; } = 1;
        }
        #endregion

        #region Tokenizer
        class Tokenizer
        {
            private static CharStream _cs;
            public static Queue<Token> Tokenize(StreamReader reader)
            {
                _cs = new CharStream(reader);
                try
                {
                    SkipSpaces();
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }

                Queue<Token> tokens = new Queue<Token>();

                bool capturing = false;
                bool textCapturing = false;
                StringBuilder capturedText = new StringBuilder("");
                while (_cs.InBounds())
                {
                    char c = _cs.Dequeue();
                    if (textCapturing)
                    {
                        if (c == '\"')
                        {
                            tokens.Enqueue(new Token(capturedText.ToString().Trim(), LexemLocation.RowNumber));
                            SkipSpaces();
                            capturedText = new StringBuilder("");
                            capturing = false;
                            textCapturing = false;
                            continue;
                        }
                    }
                    else if (c == '\"')
                    {
                        textCapturing = !textCapturing;
                        continue;
                    }
                    else if (c == '\n')
                    {
                        tokens.Enqueue(new Token(capturedText.ToString().Trim(), LexemLocation.RowNumber));
                        SkipSpaces();
                        capturedText = new StringBuilder("");
                        capturing = false;
                        continue;
                    }
                    else if (c == '=')
                    {
                        tokens.Enqueue(new Token(capturedText.ToString().Trim(), LexemLocation.RowNumber));
                        SkipSpaces();
                        tokens.Enqueue(new Token(c.ToString().Trim(), LexemLocation.RowNumber));
                        capturedText = new StringBuilder("");
                        continue;
                    }
                    else if (c == '{')
                    {
                        if (capturing)
                        {
                            tokens.Enqueue(new Token(capturedText.ToString().Trim(), LexemLocation.RowNumber));
                            SkipSpaces();
                            tokens.Enqueue(new Token(c.ToString().Trim(), LexemLocation.RowNumber));
                            capturedText = new StringBuilder("");
                            capturing = false;
                            continue;
                        }
                        else
                        {
                            capturing = true;
                        }
                    }
                    else
                    {
                        capturing = true;
                    }
                    capturedText.Append(c);
                }
                LexemLocation.RowNumber = 1;
                return tokens;
            }
            private static void SkipSpaces()
            {
                while (_cs.InBounds() && IsSpace(_cs.Peek()))
                    _cs.Dequeue();
                if (!_cs.InBounds())
                    return;
            }
            private static bool IsSpace(char ch)
            {
                return (ch == ' ') || (ch == '\n') || (ch == '\t') || (ch == 10) || (ch == 13);
            }
        }
        public class Token
        {
            public string Value { get; set; }
            public int Row { get; set; }
            public Token(string value, int row)
            {
                Value = value;
                Row = row;
            }
        }
        #endregion

        #region TestData
        public class TestData
        {
            public Test Test { get; set; } = new Test();
            public List<Question> Questions { get; set; } = new List<Question>();
            public List<Option> Options { get; set; } = new List<Option>();
            public List<Code> Codes { get; set; } = new List<Code>();
        }
        #endregion

        #region Parser
        class Parser
        {
            #region Поля
            private static Hashtable _types;
            #endregion

            #region Consume
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
            #endregion

            #region ParseBlock
            public static TestData Parse(Queue<Token> tokens)
            {
                _types = Hashtable.Synchronized(new Hashtable());
                _types["single"] = 1;
                _types["multi"] = 2;
                _types["text"] = 3;
                _types["dnd"] = 4;
                _types["code"] = 5;
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
            private static TestData ParseTest(Queue<Token> tokens)
            {
                TestData testData = new TestData();
                var test = new Test();
                bool textParsed = false, flagParsed = false, shuffleParsed = false, hideParsed = false;
                int Row = tokens.Peek().Row;
                Consume(tokens, "test");
                Consume(tokens, "{");
                while (tokens.Peek().Value != "}")
                    switch (tokens.Peek().Value)
                    {
                        case ("text"):
                            if (!textParsed)
                                test.Name = ParseString(tokens, "text");
                            else
                                throw new Exception("Text already parsed");
                            textParsed = true;
                            break;
                        case ("flag"):
                            if (!flagParsed)
                                test.IsEnabled = ParseFlag(tokens, "flag");
                            else
                                throw new Exception("Flag already parsed");
                            flagParsed = true;
                            break;
                        case ("shuffle"):
                            if (!shuffleParsed)
                                test.Shuffled = ParseFlag(tokens, "shuffle");
                            else
                                throw new Exception("Shuffle already parsed");
                            shuffleParsed = true;
                            break;
                        case ("hide"):
                            if (!hideParsed)
                                test.HideRightAnswers = ParseFlag(tokens, "hide");
                            else
                                throw new Exception("Hide already parsed");
                            hideParsed = true;
                            break;
                        case ("question"):
                            ParseQuestion(tokens, test, testData);
                            break;
                        default:
                            throw new Exception(String.Format("Неизвестный параметр: {0} (Row - {1}).", tokens.Peek().Value, tokens.Peek().Row));
                    }
                Consume(tokens, "}");
                testData.Test = test;
                if (!flagParsed)
                    test.IsEnabled = false;
                if (!shuffleParsed)
                    test.Shuffled = false;
                if (!hideParsed)
                    test.HideRightAnswers = false;
                if (!textParsed)
                    throw new Exception(String.Format("Заданы не все требуемые поля (Test (Row - {0})). Text - {1}.", Row, textParsed));
                return testData;
            }
            private static void ParseQuestion(Queue<Token> tokens, Test test, TestData testData)
            {
                Question question;
                int i = 1, checkedCount = 0, Row = 0;
                bool textParsed = false, scoreParsed = false, optionParsed = false, codeParsed = false;
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
                    case (int)Question.QuestionTypeEnum.CodeQuestion:
                        question = new CodeQuestion();
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
                                question.Title = ParseString(tokens, "text");
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
                                    throw new Exception(String.Format("TextQuestion поддерживает только один ответ (Quesiton (Row - {0})).", Row));
                            else
                                ParseOption(tokens, question, testData, i++, ref checkedCount);
                            optionParsed = true;
                            break;
                        case ("code"):
                            if (question is CodeQuestion)
                            {
                                if (!codeParsed)
                                {
                                    ParseCode(tokens, question, testData);
                                }
                                else
                                    throw new Exception(String.Format("CodeQuestion поддерживает только один ответ (Quesiton (Row - {0})).", Row));
                            }
                            else
                            {
                                throw new Exception(String.Format("Данный тип вопроса не поддерживает code(Quesiton (Row - {0})).", Row));
                            }
                            codeParsed = true;
                            break;
                        default:
                            throw new Exception(String.Format("Неизвестный параметр: {0} (Row - {1}).", tokens.Peek().Value, tokens.Peek().Row));
                    }
                question.QuestionType = Enum.GetName(typeof(Question.QuestionTypeEnum), type);
                if (question is SingleChoiceQuestion && checkedCount != 1)
                {
                    throw new Exception(String.Format("В данном типе вопроса нужно отметить верный один ответ (Quesiton (Row - {0})).", Row));
                }
                else if (question is MultiChoiceQuestion && checkedCount < 1)
                    throw new Exception(String.Format("В данном типе вопроса нужно отметить хотя бы один верный ответ (Quesiton (Row - {0})).", Row));
                Consume(tokens, "}");
                if (!(question is CodeQuestion))
                {
                    if (!textParsed || !scoreParsed || !optionParsed)
                        throw new Exception(String.Format("Заданы не все требуемые поля (Quesiton (Row - {0})). Text - {1}, Score - {2}, Option - {3}.", Row, textParsed, scoreParsed, optionParsed));
                }
                else
                    if (!textParsed || !scoreParsed || !codeParsed)
                        throw new Exception(String.Format("Заданы не все требуемые поля (Quesiton (Row - {0})). Text - {1}, Score - {2}, Code - {3}.", Row, textParsed, scoreParsed, codeParsed));
                testData.Questions.Add(question);
            }
            private static void ParseOption(Queue<Token> tokens, Question question, TestData testData, int i, ref int checkedCount)
            {
                int Row = tokens.Peek().Row;
                bool textParsed = false, flagParsed = false;
                Option option = new Option { Question = question };
                Consume(tokens, "option");
                Consume(tokens, "{");
                while (tokens.Peek().Value != "}")
                    switch (tokens.Peek().Value)
                    {
                        case ("text"):
                            if (!textParsed)
                                option.Text = ParseString(tokens, "text");
                            else
                                throw new Exception("Text already parsed");
                            textParsed = true;
                            break;
                        case ("flag"):
                            if (question is SingleChoiceQuestion || question is MultiChoiceQuestion)
                            {
                                if (!flagParsed)
                                {
                                    option.IsRight = ParseFlag(tokens, "flag");
                                    if (option.IsRight)
                                        checkedCount++;
                                }
                                else
                                    throw new Exception("Flag already parsed");
                                flagParsed = true;
                            }
                            else
                                throw new Exception(String.Format("Ответ в данном типе вопроса не поддерживает параметр flag (Option (Row - {0}))", Row));
                            break;
                        default:
                            throw new Exception(String.Format("Неизвестный параметр: {0} (Row - {1}).", tokens.Peek().Value, tokens.Peek().Row));
                    }
                Consume(tokens, "}");
                if (!textParsed)
                    throw new Exception(String.Format("Заданы не все требуемые поля (Option (Row - {0})). Text - {1}.", Row, textParsed));
                if (question is DragAndDropQuestion)
                    option.Order = i;
                else if (question is TextQuestion)
                    (question as TextQuestion).TextRightAnswer = option.Text;
                else if (!flagParsed)
                    throw new Exception(String.Format("Заданы не все требуемые поля (Option (Row - {0})). Text - {1}, Flag - {2}.", Row, textParsed, flagParsed));
                testData.Options.Add(option);
            }
            private static void ParseCode(Queue<Token> tokens, Question question, TestData testData)
            {
                int Row = tokens.Peek().Row;
                bool textParsed = false, argsParsed = false, outputParsed = false;
                Option option = new Option { Question = question };
                Code code = new Code { Question = question };
                Consume(tokens, "code");
                Consume(tokens, "{");
                while (tokens.Peek().Value != "}")
                    switch (tokens.Peek().Value)
                    {
                        case ("text"):
                            if (!textParsed)
                            {
                                string[] tmp = ParseString(tokens, "text").Split("\\n");
                                StringBuilder sb = new StringBuilder();
                                foreach (var s in tmp)
                                    sb.AppendLine(s);
                                code.Value = sb.ToString();
                            }
                            else
                                throw new Exception("Text already parsed");
                            textParsed = true;
                            break;
                        case ("args"):
                            if (!argsParsed)
                            {
                                code.Args = ParseString(tokens, "args");
                            }
                            else
                                throw new Exception("Args already parsed");
                            argsParsed = true;
                            break;
                        case ("output"):
                            if (!outputParsed)
                            {
                                code.Output = ParseString(tokens, "output");
                                option.Text = code.Output;
                            }
                            else
                                throw new Exception("Output already parsed");
                            outputParsed = true;
                            break;
                        default:
                            throw new Exception(String.Format("Неизвестный параметр: {0} (Row - {1}).", tokens.Peek().Value, tokens.Peek().Row));
                    }
                Consume(tokens, "}");
                if (!textParsed || !argsParsed || !outputParsed)
                    throw new Exception(String.Format("Заданы не все требуемые поля (Code (Row - {0})). Text - {1}, Args - {2}, Output - {3}.", Row, textParsed, argsParsed, outputParsed));
                testData.Options.Add(option);
                testData.Codes.Add(code);
            }
            #endregion

            #region ParseExrp
            private static string ParseString(Queue<Token> tokens, string tok)
            {
                Consume(tokens, tok);
                Consume(tokens, "=");
                string output = tokens.Dequeue().Value;
                return output;
            }
            private static bool ParseFlag(Queue<Token> tokens, string tok)
            {
                Consume(tokens, tok);
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
            #endregion

        }
        #endregion
    }
}
