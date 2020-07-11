using System;

namespace XiLang.Exceptions
{

    [Serializable]
    public class XiLangException : Exception
    {
        public int Line { get; }

        public XiLangException(string message, int line) : base(message)
        {
            Line = line;
        }
    }
}
