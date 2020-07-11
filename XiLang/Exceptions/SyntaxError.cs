using System;
using XiLang.Lexical;

namespace XiLang.Exceptions
{
    [Serializable]
    public class SyntaxException : XiLangException
    {
        public Token Token { get; }

        public SyntaxException(string msg, Token t) : base(msg, t.Line)
        {
            Token = t;
        }
    }
}
