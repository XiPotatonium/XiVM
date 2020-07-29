using System;

namespace XiLang.Errors
{

    [Serializable]
    public class XiLangError : Exception
    {
        public int Line { get; }

        public XiLangError(string message, int line = -1) : base(message)
        {
            Line = line;
        }
    }
}
