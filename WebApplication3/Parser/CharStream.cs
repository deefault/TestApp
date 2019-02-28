using System;
using System.IO;

namespace WebApplication3.Parser
{
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
            //if (ch == '\n')
            //{
            //    LexemLocation.rowNumber++;
            //    LexemLocation.colNumber = 1;
            //}
            //else
            //{
            //    LexemLocation.colNumber++;
            //}
            return ch;
        }
    }
}
