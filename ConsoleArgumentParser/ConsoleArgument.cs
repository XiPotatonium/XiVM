using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleArgumentParser
{
    public enum ArgumentValueType
    {
        NONE, STRING, INT
    }

    public class ConsoleArgument
    {
        public string FullName { get; }
        public string Alias { get; }

        public ArgumentValueType ValueType { private set; get; }

        public string StringValue { set; get; }
        public int IntValue { set; get; }

        public bool IsSet { set; get; } = false;

        /// <summary>
        /// 默认规则的argument，需要是string
        /// </summary>
        public ConsoleArgument()
        {
            ValueType = ArgumentValueType.STRING;
        }

        public ConsoleArgument(string fullName, ArgumentValueType type = ArgumentValueType.NONE)
        {
            FullName = fullName;
            ValueType = type;
        }

        public ConsoleArgument(string fullName, string alias, ArgumentValueType type = ArgumentValueType.NONE)
        {
            FullName = fullName;
            Alias = alias;
            ValueType = type;
        }
    }
}
