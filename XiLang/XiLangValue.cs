using System;

namespace XiLang
{
    public class XiLangValue
    {
        private static readonly XiLangValue NullValue = new XiLangValue()
        {
            Type = ValueType.NULL
        };

        private static readonly XiLangValue TrueValue = new XiLangValue()
        {
            Type = ValueType.BOOL,
            IntVal = 1
        };

        private static readonly XiLangValue FalseValue = new XiLangValue()
        {
            Type = ValueType.BOOL,
            IntVal = 0
        };

        private static readonly XiLangValue DefaultInt = new XiLangValue()
        {
            Type = ValueType.INT,
            IntVal = 0
        };

        private static readonly XiLangValue DefaultFloat = new XiLangValue()
        {
            Type = ValueType.FLOAT,
            FloatVal = 0.0
        };

        private static readonly XiLangValue DefaultString = new XiLangValue()
        {
            Type = ValueType.STR,
            StrVal = string.Empty
        };

        public static XiLangValue MakeNull()
        {
            return NullValue;
        }

        public static XiLangValue MakeBool(bool val)
        {
            return val ? TrueValue : FalseValue;
        }

        public static XiLangValue MakeInt(string literal, int fromBase = 10)
        {
            return new XiLangValue()
            {
                Type = ValueType.INT,
                IntVal = Convert.ToInt64(literal, fromBase)
            };
        }

        public static XiLangValue MakeFloat(string literal)
        {
            return new XiLangValue()
            {
                Type = ValueType.FLOAT,
                FloatVal = Convert.ToDouble(literal)
            };
        }

        public static XiLangValue GetDefault(ValueType type)
        {
            return type switch
            {
                ValueType.INT => DefaultInt,
                ValueType.FLOAT => DefaultFloat,
                ValueType.STR => DefaultString,
                ValueType.BOOL => FalseValue,
                ValueType.NULL => NullValue,
                _ => null,
            };
        }

        /// <summary>
        /// TODO 转义字符
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public static XiLangValue MakeString(string literal)
        {
            literal = literal[1..^1];
            return new XiLangValue()
            {
                Type = ValueType.STR,
                StrVal = literal
            };
        }

        public ValueType Type { set; get; }
        public string StrVal { set; get; }
        public long IntVal { set; get; }
        public double FloatVal { set; get; }
    }
}
