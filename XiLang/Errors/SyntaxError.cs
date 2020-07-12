using System;
using XiLang.Lexical;

namespace XiLang.Errors
{
    [Serializable]
    public class SyntaxError : XiLangError
    {
        public Token Token { get; }

        public SyntaxError(string msg, Token t) : base(msg, t.Line)
        {
            Token = t;
        }
    }
}
