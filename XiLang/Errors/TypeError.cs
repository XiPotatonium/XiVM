using System;
using System.Collections.Generic;
using System.Text;

namespace XiLang.Errors
{
    [Serializable]
    public class TypeError : XiLangError
    {
        public static TypeError ExpectType(string owner, ValueType real, int line, params ValueType[] expect)
        {
            StringBuilder sb = new StringBuilder("Expect ").Append(expect[0].ToString());
            for (int i = 1; i < expect.Length; ++i)
            {
                sb.Append("/").Append(expect[i].ToString());
            }
            return new TypeError($"{owner} expect {expect} but found {real}", line);
        }

        public TypeError(string message, int line) : base(message, line) { }
    }
}
