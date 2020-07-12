using System;

namespace XiLang.Errors
{
    [Serializable]
    public class LexicalException : XiLangError
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
