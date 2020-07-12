using System;
using XiLang.Lexical;

namespace XiLang.Errors
{
    [Serializable]
    public class SyntaxException : XiLangError
    {
        public Token Token { get; }

        public SyntaxException(string msg, Token t) : base(msg, t.Line)
        {
            Token = t;
        }
    }
}
