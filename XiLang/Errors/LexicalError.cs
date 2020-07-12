using System;

namespace XiLang.Errors
{
    [Serializable]
    public class LexicalError : XiLangError
    {
        public int Column { get; }
        public string LineStr { get; }
        public LexicalError(int line, int col, string lineStr) : base("Lexical Error", line)
        {
            Column = col;
            LineStr = lineStr;
        }
    }

}
