using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace WebApplication3.Parser
{
    class Tokenizer
    {
        private static CharStream _cs;
        public static Queue<string> Tokenize(StreamReader _reader)
        {
            _cs = new CharStream(_reader);
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

                if (c == '<')
                {
                    if (capturing)
                    {
                        tokens.Enqueue(capturedText.ToString());
                        SkipSpaces();
                    }
                    else
                    {
                        capturing = true;
                    }

                    capturedText = new StringBuilder("");
                }
                else if (c == '>')
                {
                    capturing = false;
                    capturedText.Append(c);
                    tokens.Enqueue(capturedText.ToString());
                    SkipSpaces();
                }
                else if (!capturing)
                {
                    capturedText = new StringBuilder("");
                    capturing = true;
                }
                if (capturing)
                {
                    capturedText.Append(c);
                }
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
