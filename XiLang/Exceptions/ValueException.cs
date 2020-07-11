using System;

namespace XiLang.Exceptions
{
    [Serializable]
    public class ValueException : XiLangException
    {
        public ValueException(string message, int line) : base(message, line) { }
    }
}
