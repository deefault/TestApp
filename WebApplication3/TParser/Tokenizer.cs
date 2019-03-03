using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebApplication3.TParser
{
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
            StringBuilder capturedText = new StringBuilder("");
            while (_cs.InBounds())
            {
                char c = _cs.Dequeue();

                if (c == '\n')
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
}
