using System;
using System.Text;
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
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < literal.Length - 1; ++i)
            {
                if (literal[i] == '\\')
                {
                    ++i;
                    // TODO 更多的转义字符
                    sb.Append((char)((literal[i]) switch
                    {
                        'n' => 10,
                        _ => throw new NotImplementedException(),
                    }));
                }
                else
                {
                    sb.Append(literal[i]);
                }
            }
            return new XiLangValue()
            {
                Type = ValueType.STRING,
                StringValue = sb.ToString()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        public static XiLangValue MakeChar(string literal)
        {
            int value;
            if (literal[1] == '\\')
            {
                // TODO 更多的转义字符
                value = (literal[2]) switch
                {
                    'n' => 10,
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                value = literal[1];
            }

            return new XiLangValue()
            {
                Type = ValueType.INT,
                IntValue = value
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
