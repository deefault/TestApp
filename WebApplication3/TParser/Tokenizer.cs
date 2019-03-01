using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebApplication3.TParser
{
    class Tokenizer
    {
        private static CharStream _cs;
        public static Queue<string> Tokenize(StreamReader reader)
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

            Queue<string> tokens = new Queue<string>();

            bool capturing = false;
            StringBuilder capturedText = new StringBuilder("");
            while (_cs.InBounds())
            {
                char c = _cs.Dequeue();

                if (c == '\n')
                {
                    tokens.Enqueue(capturedText.ToString().Trim());
                    SkipSpaces();
                    capturedText = new StringBuilder("");
                    capturing = false;
                    continue;
                }
                else if (c == '=')
                {
                    tokens.Enqueue(capturedText.ToString().Trim());
                    SkipSpaces();
                    tokens.Enqueue(c.ToString());
                    capturedText = new StringBuilder("");
                    continue;
                }
                else if (c == '{')
                {
                    if (capturing)
                    {
                        tokens.Enqueue(capturedText.ToString().Trim());
                        SkipSpaces();
                        tokens.Enqueue(c.ToString());
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

}
