using System;
using XiLang.Errors;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{

    internal partial class Expr : AST
    {
        public virtual bool IsConst()
        {
            if (Expr1 != null && !Expr1.IsConst())
            {
                return false;
            }
            if (Expr2 != null && !Expr2.IsConst())
            {
                return false;
            }
            if (Expr3 != null && !Expr3.IsConst())
            {
                return false;
            }
            return true;
        }

        public virtual XiLangValue EvaluateConstExpr()
        {
            XiLangValue v1 = Expr1?.EvaluateConstExpr();
            XiLangValue v2 = Expr2?.EvaluateConstExpr();

            switch (OpType)
            {
                case OpType.NEG:
                    if (v1.Type == ValueType.INT)
                    {
                        v1.IntValue = -v1.IntValue;
                    }
                    else if (v1.Type == ValueType.DOUBLE)
                    {
                        v1.DoubleValue = -v1.DoubleValue;
                    }
                    else
                    {
                        throw TypeError.ExpectType(OpType.ToString(), v1.Type, Line, ValueType.INT, ValueType.DOUBLE);
                    }
                    break;
                case OpType.ADD:
                case OpType.SUB:
                case OpType.MUL:
                case OpType.DIV:
                case OpType.MOD:
                    if (v1.Type == ValueType.DOUBLE && v2.Type == ValueType.DOUBLE)
                    {
                        v1.DoubleValue = OpType switch
                        {
                            OpType.ADD => v1.DoubleValue + v2.DoubleValue,
                            OpType.SUB => v1.DoubleValue - v2.DoubleValue,
                            OpType.MUL => v1.DoubleValue * v2.DoubleValue,
                            OpType.DIV => v1.DoubleValue / v2.DoubleValue,
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else if (v1.Type == ValueType.INT && v2.Type == ValueType.INT)
                    {
                        v1.IntValue = OpType switch
                        {
                            OpType.ADD => v1.IntValue + v2.IntValue,
                            OpType.SUB => v1.IntValue - v2.IntValue,
                            OpType.MUL => v1.IntValue * v2.IntValue,
                            OpType.DIV => v1.IntValue / v2.IntValue,
                            OpType.MOD => v1.IntValue % v2.IntValue,
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else if (v1.Type == ValueType.STRING && v2.Type == ValueType.STRING &&
                        OpType == OpType.ADD)
                    {
                        // 字符串字面量的拼接
                        v1.StringValue += v2.StringValue;
                    }
                    else
                    {
                        throw new TypeError($"{OpType} cannot be applied to {v1.Type} and {v2.Type}", Line);
                    }
                    break;
                case OpType.LOG_NOT:
                    if (v1.Type == ValueType.BOOL)
                    {
                        v1 = XiLangValue.MakeBool(!v1.BoolValue);
                    }
                    else
                    {
                        throw TypeError.ExpectType(OpType.ToString(), v1.Type, Line, ValueType.BOOL);
                    }
                    break;
                case OpType.LOG_AND:
                case OpType.LOG_OR:
                    if (v1.Type != ValueType.BOOL)
                    {
                        throw TypeError.ExpectType(OpType.ToString(), v1.Type, Line, ValueType.BOOL);
                    }
                    else if (v2.Type != ValueType.BOOL)
                    {
                        throw TypeError.ExpectType(OpType.ToString(), v2.Type, Line, ValueType.BOOL);
                    }
                    else
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.LOG_AND => v1.BoolValue && v2.BoolValue,
                            OpType.LOG_OR => v1.BoolValue || v2.BoolValue,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    break;
                case OpType.BIT_NOT:
                case OpType.BIT_AND:
                case OpType.BIT_XOR:
                case OpType.BIT_OR:
                case OpType.BIT_SL:
                case OpType.BIT_SR:
                    // bit运算不在计划中
                    throw new NotImplementedException();
                case OpType.EQ:
                case OpType.NE:
                case OpType.GE:
                case OpType.GT:
                case OpType.LE:
                case OpType.LT:
                    if (v1.Type == ValueType.DOUBLE && v2.Type == ValueType.DOUBLE)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => v1.DoubleValue == v2.DoubleValue,
                            OpType.NE => v1.DoubleValue != v2.DoubleValue,
                            OpType.GE => v1.DoubleValue >= v2.DoubleValue,
                            OpType.GT => v1.DoubleValue > v2.DoubleValue,
                            OpType.LE => v1.DoubleValue <= v2.DoubleValue,
                            OpType.LT => v1.DoubleValue < v2.DoubleValue,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else if (v1.Type == ValueType.INT && v2.Type == ValueType.INT)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => v1.IntValue == v2.IntValue,
                            OpType.NE => v1.IntValue != v2.IntValue,
                            OpType.GE => v1.IntValue >= v2.IntValue,
                            OpType.GT => v1.IntValue > v2.IntValue,
                            OpType.LE => v1.IntValue <= v2.IntValue,
                            OpType.LT => v1.IntValue < v2.IntValue,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else if (v1.Type == ValueType.STRING && v2.Type == ValueType.STRING)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => string.Compare(v1.StringValue, v2.StringValue) == 0,
                            OpType.NE => string.Compare(v1.StringValue, v2.StringValue) != 0,
                            OpType.GE => string.Compare(v1.StringValue, v2.StringValue) >= 0,
                            OpType.GT => string.Compare(v1.StringValue, v2.StringValue) > 0,
                            OpType.LE => string.Compare(v1.StringValue, v2.StringValue) <= 0,
                            OpType.LT => string.Compare(v1.StringValue, v2.StringValue) < 0,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else if (v1.Type == ValueType.BOOL && v2.Type == ValueType.BOOL)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => v1.BoolValue == v2.BoolValue,
                            OpType.NE => v1.BoolValue != v2.BoolValue,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else if (v1.Type == ValueType.NULL)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => v2.Type == ValueType.NULL,
                            OpType.NE => v2.Type != ValueType.NULL,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else if (v2.Type == ValueType.NULL)
                    {
                        v1 = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.EQ => v1.Type == ValueType.NULL,
                            OpType.NE => v1.Type != ValueType.NULL,
                            _ => throw new NotImplementedException(),
                        });
                    }
                    else
                    {
                        throw new TypeError($"{OpType} cannot be applied to {v1.Type} and {v2.Type}", Line);
                    }
                    break;
                case OpType.CONDITIONAL:
                    throw new NotImplementedException();
                case OpType.CAST:
                    v1 = XiLangValue.Cast(((TypeExpr)Expr1).Type switch
                    {
                        SyntacticValueType.BOOL => ValueType.BOOL,
                        SyntacticValueType.INT => ValueType.INT,
                        SyntacticValueType.DOUBLE => ValueType.DOUBLE,
                        SyntacticValueType.STRING => ValueType.STRING,
                        _ => throw new NotImplementedException(),
                    }, v2, Expr1.Line);
                    break;
                default:
                    throw new XiLangError($"{OpType} is not supported in const expression", Line);
            }
            return v1;
        }
    }



    internal class ConstExpr : Expr
    {
        #region Static Make

        public static ConstExpr MakeNull(int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeNull()
            };
        }


        public static ConstExpr MakeBool(bool val, int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeBool(val)
            };
        }

        public static ConstExpr MakeInt(string literal, int fromBase, int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeInt(literal, fromBase)
            };
        }

        public static ConstExpr MakeFloat(string literal, int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeDouble(literal)
            };
        }

        public static ConstExpr MakeString(string literal, int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeString(literal)
            };
        }

        public static ConstExpr MakeChar(string literal, int line)
        {
            return new ConstExpr(line)
            {
                Value = XiLangValue.MakeChar(literal)
            };
        }

        #endregion

        public ConstExpr(int line)
            : base(line)
        {

        }

        public XiLangValue Value { set; get; }

        public override bool IsConst()
        {
            return true;
        }

        public override string ASTLabel()
        {
            return Value.Type switch
            {
                ValueType.INT => Value.IntValue.ToString(),
                ValueType.DOUBLE => Value.DoubleValue.ToString(),
                ValueType.STRING => $"\\\"{Value.StringValue}\\\"",
                ValueType.BOOL => Value.IntValue == 0 ? "false" : "true",
                ValueType.NULL => "null",
                _ => throw new NotImplementedException(),
            };
        }

        public override XiLangValue EvaluateConstExpr()
        {
            return Value;
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            switch (Value.Type)
            {
                case ValueType.INT:
                    pass.Constructor.AddPushI(Value.IntValue);
                    return VariableType.IntType;
                case ValueType.DOUBLE:
                    pass.Constructor.AddPushD(Value.DoubleValue);
                    return VariableType.DoubleType;
                case ValueType.STRING:
                    pass.Constructor.AddConst(pass.Constructor.StringPool.TryAdd(Value.StringValue));
                    // TODO
                    return VariableType.AddressType;
                case ValueType.BOOL:
                    pass.Constructor.AddPushB(Value.BoolValue ? (byte)1 : (byte)0);
                    return VariableType.ByteType;
                case ValueType.NULL:
                    pass.Constructor.AddPushA(0);
                    return VariableType.AddressType;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
