using System;

namespace XiLang.Exceptions
{
    [Serializable]
    public class LexicalException : XiLangException
    {
        public int Column { get; }
        public string LineStr { get; }
        public LexicalException(int line, int col, string lineStr) : base("Lexical Error", line)
        {
            Column = col;
            LineStr = lineStr;
        }
    }

}
