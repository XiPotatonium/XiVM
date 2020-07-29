using System;
using XiLang.Errors;

namespace XiLang
{
    public class XiLangValue
    {
        public static readonly XiLangValue NullValue = new XiLangValue()
        {
            Type = ValueType.NULL
        };

        public static readonly XiLangValue TrueValue = new XiLangValue()
        {
            Type = ValueType.BOOL,
            IntValue = 1
        };

        public static readonly XiLangValue FalseValue = new XiLangValue()
        {
            Type = ValueType.BOOL,
            IntValue = 0
        };

        private static readonly XiLangValue DefaultInt = new XiLangValue()
        {
            Type = ValueType.INT,
            IntValue = 0
        };

        private static readonly XiLangValue DefaultDouble = new XiLangValue()
        {
            Type = ValueType.DOUBLE,
            DoubleValue = 0.0f
        };

        private static readonly XiLangValue DefaultString = new XiLangValue()
        {
            Type = ValueType.STRING,
            StringValue = string.Empty
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
                IntValue = Convert.ToInt32(literal, fromBase)
            };
        }

        public static XiLangValue MakeDouble(string literal)
        {
            return new XiLangValue()
            {
                Type = ValueType.DOUBLE,
                DoubleValue = (float)Convert.ToDouble(literal)
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
                Type = ValueType.STRING,
                StringValue = literal
            };
        }

        public static XiLangValue GetDefault(ValueType type)
        {
            return type switch
            {
                ValueType.INT => DefaultInt,
                ValueType.DOUBLE => DefaultDouble,
                ValueType.STRING => DefaultString,
                ValueType.BOOL => FalseValue,
                ValueType.NULL => NullValue,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// 目前仅允许float和int相互cast
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static XiLangValue Cast(ValueType type, XiLangValue value, int line = -1)
        {
            if (value.Type == type)
            {
                return value;
            }
            else if (type == ValueType.DOUBLE)
            {
                if (value.Type == ValueType.INT)
                {
                    return new XiLangValue() { Type = type, DoubleValue = value.IntValue };
                }
            }
            else if (type == ValueType.INT)
            {
                if (value.Type == ValueType.DOUBLE)
                {
                    return new XiLangValue() { Type = type, IntValue = (int)value.DoubleValue };
                }
            }
            throw new TypeError($"Cannot cast from {value.Type} to {type}", line);
        }

        public ValueType Type { set; get; }
        public string StringValue { set; get; }
        public int IntValue { set; get; }
        public float DoubleValue { set; get; }
        public bool BoolValue => this == TrueValue;
    }
}
