using System;

namespace XiLang.Errors
{
    [Serializable]
    public class ClassError : XiLangError
    {
        public ClassError(string msg, int line) : base(msg, line)
        {

        }
    }
}
