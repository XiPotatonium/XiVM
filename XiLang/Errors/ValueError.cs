using System;

namespace XiLang.Errors
{
    [Serializable]
    public class ValueError : XiLangError
    {
        public ValueError(string message, int line) : base(message, line) { }
    }
}
