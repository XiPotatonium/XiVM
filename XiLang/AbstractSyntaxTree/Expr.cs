using System;
using XiLang.Errors;
using XiVM;
using XiVM.Xir.Symbol;

namespace XiLang.AbstractSyntaxTree
{
    public enum ExprType
    {
        CONST,
        ID,
        OPEXPR
    }

    public class Expr : AST
    {
        #region Static Make

        public static Expr MakeNull(int line)
        {
            return new Expr(ExprType.CONST, line)
            {
                Value = XiLangValue.MakeNull()
            };
        }


        public static Expr MakeBool(bool val, int line)
        {
            return new Expr(ExprType.CONST, line)
            {
                Value = XiLangValue.MakeBool(val)
            };
        }

        public static Expr MakeInt(string literal, int fromBase, int line)
        {
            return new Expr(ExprType.CONST, line)
            {
                Value = XiLangValue.MakeInt(literal, fromBase)
            };
        }

        public static Expr MakeFloat(string literal, int line)
        {
            return new Expr(ExprType.CONST, line)
            {
                Value = XiLangValue.MakeDouble(literal)
            };
        }

        public static Expr MakeString(string literal, int line)
        {
            return new Expr(ExprType.CONST, line)
            {
                Value = XiLangValue.MakeString(literal)
            };
        }

        /// <summary>
        /// 约定id值放在Value.StrVal里
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Expr MakeId(string id, int line)
        {
            return new Expr(ExprType.ID, line)
            {
                Value = new XiLangValue()
                {
                    StringValue = id
                }
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs, int line)
        {
            return new Expr(ExprType.OPEXPR, line)
            {
                OpType = type,
                Expr1 = lhs
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs, Expr rhs, int line)
        {
            return new Expr(ExprType.OPEXPR, line)
            {
                OpType = type,
                Expr1 = lhs,
                Expr2 = rhs
            };
        }

        public static Expr MakeOp(OpType type, Expr expr1, Expr expr2, Expr expr3, int line)
        {
            return new Expr(ExprType.OPEXPR, line)
            {
                OpType = type,
                Expr1 = expr1,
                Expr2 = expr2,
                Expr3 = expr3
            };
        }
        #endregion

        public int Line { private set; get; }
        public ExprType ExprType { set; get; }
        public OpType OpType { set; get; }
        public Expr Expr1 { set; get; }
        public Expr Expr2 { set; get; }
        public Expr Expr3 { set; get; }

        public XiLangValue Value { set; get; }

        protected Expr() { }
        private Expr(ExprType type, int line)
        {
            Line = line;
            ExprType = type;
        }

        public bool IsConst()
        {
            if (ExprType == ExprType.ID)
            {
                return false;
            }
            if (ExprType == ExprType.OPEXPR)
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
            }
            return true;
        }

        private void AssertValueType(string owner, ValueType type)
        {
            if (Value.Type != type)
            {
                throw TypeError.ExpectType(owner, Value.Type, Line, type);
            }
        }

        /// <summary>
        /// EvaluateConstExpr不在乎op是修改原先的value还是重建一个value
        /// </summary>
        /// <returns></returns>
        public XiLangValue EvaluateConstExpr()
        {
            if (ExprType == ExprType.OPEXPR)
            {
                if (Expr1 != null)
                {
                    Expr1.EvaluateConstExpr();
                }
                if (Expr2 != null)
                {
                    Expr2.EvaluateConstExpr();
                }
                if (Expr3 != null)
                {
                    Expr3.EvaluateConstExpr();
                }
                switch (OpType)
                {
                    case OpType.NEG:
                        Value = Expr1.Value;
                        if (Value.Type == ValueType.INT)
                        {
                            Value.IntValue = -Value.IntValue;
                        }
                        else if (Value.Type == ValueType.DOUBLE)
                        {
                            Value.DoubleValue = -Value.DoubleValue;
                        }
                        else
                        {
                            throw TypeError.ExpectType(OpType.ToString(), Value.Type, Line, ValueType.INT, ValueType.DOUBLE);
                        }
                        break;
                    case OpType.ADD:
                    case OpType.SUB:
                    case OpType.MUL:
                    case OpType.DIV:
                    case OpType.MOD:
                        if (Expr1.Value.Type == ValueType.DOUBLE || Expr2.Value.Type == ValueType.DOUBLE)
                        {
                            Value = XiLangValue.Cast(ValueType.DOUBLE, Expr1.Value, Expr1.Line);
                            Expr2.Value = XiLangValue.Cast(ValueType.DOUBLE, Expr2.Value, Expr2.Line);
                            Value.DoubleValue = OpType switch
                            {
                                OpType.ADD => Value.DoubleValue + Expr2.Value.DoubleValue,
                                OpType.SUB => Value.DoubleValue - Expr2.Value.DoubleValue,
                                OpType.MUL => Value.DoubleValue * Expr2.Value.DoubleValue,
                                OpType.DIV => Value.DoubleValue / Expr2.Value.DoubleValue,
                                OpType.MOD => Value.DoubleValue % Expr2.Value.DoubleValue,
                                _ => throw new NotImplementedException(),
                            };
                        }
                        else if (Expr1.Value.Type == ValueType.INT && Expr2.Value.Type == ValueType.INT)
                        {
                            Value = Expr1.Value;
                            Value.IntValue = OpType switch
                            {
                                OpType.ADD => Value.IntValue + Expr2.Value.IntValue,
                                OpType.SUB => Value.IntValue - Expr2.Value.IntValue,
                                OpType.MUL => Value.IntValue * Expr2.Value.IntValue,
                                OpType.DIV => Value.IntValue / Expr2.Value.IntValue,
                                OpType.MOD => Value.IntValue % Expr2.Value.IntValue,
                                _ => throw new NotImplementedException(),
                            };
                        }
                        else
                        {
                            throw new TypeError($"{OpType} cannot be applied to {Expr1.Value.Type} and {Expr2.Value.Type}", Line);
                        }
                        break;
                    case OpType.LOG_NOT:
                        if (Expr1.Value.Type == ValueType.BOOL)
                        {
                            Value = XiLangValue.MakeBool(!Expr1.Value.BoolValue);
                        }
                        else
                        {
                            throw TypeError.ExpectType(OpType.ToString(), Value.Type, Line, ValueType.BOOL);
                        }
                        break;
                    case OpType.LOG_AND:
                    case OpType.LOG_OR:
                        Expr1.AssertValueType(OpType.ToString(), ValueType.BOOL);
                        Expr2.AssertValueType(OpType.ToString(), ValueType.BOOL);
                        Value = XiLangValue.MakeBool(OpType switch
                        {
                            OpType.LOG_AND => Expr1.Value.BoolValue && Expr2.Value.BoolValue,
                            OpType.LOG_OR => Expr1.Value.BoolValue || Expr2.Value.BoolValue,
                            _ => throw new NotImplementedException(),
                        });
                        break;
                    case OpType.BIT_NOT:
                    case OpType.BIT_AND:
                    case OpType.BIT_XOR:
                    case OpType.BIT_OR:
                    case OpType.BIT_SL:
                    case OpType.BIT_SR:
                        Expr1.AssertValueType(OpType.ToString(), ValueType.INT);
                        Value = Expr1.Value;
                        if (OpType == OpType.BIT_NOT)
                        {   // 一元~
                            Value.IntValue = ~Value.IntValue;
                        }
                        else
                        {   // 二元bit运算
                            Expr2.AssertValueType(OpType.ToString(), ValueType.INT);
                            Value.IntValue = OpType switch
                            {
                                OpType.BIT_AND => Value.IntValue & Expr2.Value.IntValue,
                                OpType.BIT_XOR => Value.IntValue & Expr2.Value.IntValue,
                                OpType.BIT_OR => Value.IntValue | Expr2.Value.IntValue,
                                OpType.BIT_SL => Value.IntValue << Expr2.Value.IntValue,
                                OpType.BIT_SR => Value.IntValue >> Expr2.Value.IntValue,
                                _ => throw new NotImplementedException(),
                            };
                        }
                        break;
                    case OpType.EQ:
                    case OpType.NE:
                    case OpType.GE:
                    case OpType.GT:
                    case OpType.LE:
                    case OpType.LT:
                        if (Expr1.Value.Type == ValueType.DOUBLE || Expr2.Value.Type == ValueType.DOUBLE)
                        {
                            Expr1.Value = XiLangValue.Cast(ValueType.DOUBLE, Expr1.Value, Expr1.Line);
                            Expr2.Value = XiLangValue.Cast(ValueType.DOUBLE, Expr2.Value, Expr2.Line);
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => Expr1.Value.DoubleValue == Expr2.Value.DoubleValue,
                                OpType.NE => Expr1.Value.DoubleValue != Expr2.Value.DoubleValue,
                                OpType.GE => Expr1.Value.DoubleValue >= Expr2.Value.DoubleValue,
                                OpType.GT => Expr1.Value.DoubleValue > Expr2.Value.DoubleValue,
                                OpType.LE => Expr1.Value.DoubleValue <= Expr2.Value.DoubleValue,
                                OpType.LT => Expr1.Value.DoubleValue < Expr2.Value.DoubleValue,
                                _ => throw new NotImplementedException(),
                            });
                        }
                        else if (Expr1.Value.Type == ValueType.INT && Expr2.Value.Type == ValueType.INT)
                        {
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => Expr1.Value.IntValue == Expr2.Value.IntValue,
                                OpType.NE => Expr1.Value.IntValue != Expr2.Value.IntValue,
                                OpType.GE => Expr1.Value.IntValue >= Expr2.Value.IntValue,
                                OpType.GT => Expr1.Value.IntValue > Expr2.Value.IntValue,
                                OpType.LE => Expr1.Value.IntValue <= Expr2.Value.IntValue,
                                OpType.LT => Expr1.Value.IntValue < Expr2.Value.IntValue,
                                _ => throw new NotImplementedException(),
                            });
                        }
                        else if (Expr1.Value.Type == ValueType.STRING && Expr2.Value.Type == ValueType.STRING)
                        {
                            int res = string.Compare(Expr1.Value.StringValue, Expr2.Value.StringValue);
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => res == 0,
                                OpType.NE => res != 0,
                                OpType.GE => res >= 0,
                                OpType.GT => res > 0,
                                OpType.LE => res <= 0,
                                OpType.LT => res < 0,
                                _ => throw new NotImplementedException(),
                            });
                        }
                        else if (Expr1.Value.Type == ValueType.BOOL && Expr2.Value.Type == ValueType.BOOL)
                        {
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => Expr1.Value.BoolValue == Expr2.Value.BoolValue,
                                OpType.NE => Expr1.Value.BoolValue != Expr2.Value.BoolValue,
                                _ => throw new TypeError($"{OpType} cannot be applied to {Expr1.Value.Type} and {Expr2.Value.Type}", Line),
                            });
                        }
                        else
                        {
                            throw new TypeError($"{OpType} cannot be applied to {Expr1.Value.Type} and {Expr2.Value.Type}", Line);
                        }
                        break;
                    case OpType.CONDITIONAL:
                        break;
                    case OpType.CAST:
                        Value = XiLangValue.Cast(((TypeExpr)Expr1).Type switch
                        {
                            SyntacticValueType.BOOL => ValueType.BOOL,
                            SyntacticValueType.INT => ValueType.INT,
                            SyntacticValueType.DOUBLE => ValueType.DOUBLE,
                            SyntacticValueType.STRING => ValueType.STRING,
                            _ => throw new NotImplementedException(),
                        }, Expr2.Value, Expr1.Line);
                        break;
                    default:
                        throw new XiLangError($"{OpType} is not supported in const expression", Line);
                }
            }
            return Value;
        }

        public override string ASTLabel()
        {
            return ExprType switch
            {
                ExprType.CONST => Value.Type switch
                {
                    ValueType.INT => Value.IntValue.ToString(),
                    ValueType.DOUBLE => Value.DoubleValue.ToString(),
                    ValueType.STRING => $"\\\"{Value.StringValue}\\\"",
                    ValueType.BOOL => Value.IntValue == 0 ? "false" : "true",
                    ValueType.NULL => "null",
                    _ => "UNK",
                },
                ExprType.ID => $"(Id){Value.StringValue}",
                ExprType.OPEXPR => OpType switch
                {
                    OpType.NEG => "-(neg)",
                    OpType.INC => "++",
                    OpType.DEC => "++",
                    OpType.ADD => "+",
                    OpType.SUB => "-",
                    OpType.MUL => "*",
                    OpType.DIV => "/",
                    OpType.MOD => "%",
                    OpType.LOG_NOT => "!",
                    OpType.LOG_AND => "&&",
                    OpType.LOG_OR => "||",
                    OpType.BIT_NOT => "~",
                    OpType.BIT_AND => "&",
                    OpType.BIT_OR => "|",
                    OpType.BIT_SL => "<<",
                    OpType.BIT_SR => ">>",
                    OpType.BIT_XOR => "^",
                    OpType.EQ => "==",
                    OpType.NE => "!=",
                    OpType.GE => ">=",
                    OpType.GT => ">",
                    OpType.LE => "<=",
                    OpType.LT => "<",
                    OpType.ASSIGN => "=",
                    OpType.ADD_ASSIGN => "+=",
                    OpType.SUB_ASSIGN => "-=",
                    OpType.MUL_ASSIGN => "*=",
                    OpType.DIV_ASSIGN => "/=",
                    OpType.MOD_ASSIGN => "%=",
                    OpType.AND_ASSIGN => "&",
                    OpType.OR_ASSIGN => "|",
                    OpType.XOR_ASSIGN => "^",
                    OpType.SL_ASSIGN => "<<",
                    OpType.SR_ASSIGN => ">>",
                    OpType.CONDITIONAL => "?:",
                    OpType.CALL => "(Call)",
                    OpType.CAST => "(CAST)",
                    OpType.CLASS_ACCESS => ".",
                    OpType.ARRAY_ACCESS => "[]",
                    _ => "UNK",
                },
                _ => "UNK",
            };
        }

        public override AST[] Children()
        {
            return new AST[] { Expr1, Expr2, Expr3 };
        }

        public override VariableType CodeGen()
        {
            VariableType valueType;
            switch (ExprType)
            {
                case ExprType.CONST:
                    switch (Value.Type)
                    {
                        case ValueType.INT:
                            CodeGenPass.Constructor.AddPushI(Value.IntValue);
                            return VariableType.IntType;
                        case ValueType.DOUBLE:
                            throw new NotImplementedException();
                        case ValueType.STRING:
                            throw new NotImplementedException();
                        case ValueType.BOOL:
                            throw new NotImplementedException();
                        case ValueType.NULL:
                            CodeGenPass.Constructor.AddPushA(0);
                            return VariableType.NullType;
                        default:
                            throw new NotImplementedException();
                    }
                case ExprType.ID:
                    if (CodeGenPass.Constructor.SymbolTable.TryGetValue(Value.StringValue, out Symbol symbol, out uint levelDiff))
                    {
                        if (symbol is FunctionSymbol function)
                        {
                            CodeGenPass.Constructor.AddPushA(function.Function.Index);
                            return function.Function.Type;
                        }
                        else if (symbol is VariableSymbol variable)
                        {
                            CodeGenPass.Constructor.AddGetA(levelDiff, variable.XirVariable.Offset);
                            CodeGenPass.Constructor.AddLoadT(variable.XirVariable.Type);
                            return variable.XirVariable.Type;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new XiLangError($"Variable {Value.StringValue} is not declared when used", Line);
                    }
                case ExprType.OPEXPR:
                    switch (OpType)
                    {
                        case OpType.NEG:
                            throw new NotImplementedException();
                        case OpType.INC:
                            throw new NotImplementedException();
                        case OpType.DEC:
                            throw new NotImplementedException();
                        case OpType.ADD:
                            Expr1.CodeGen();
                            Expr2.CodeGen();
                            return CodeGenPass.Constructor.AddAddI();
                        case OpType.SUB:
                            throw new NotImplementedException();
                        case OpType.MUL:
                            throw new NotImplementedException();
                        case OpType.DIV:
                            throw new NotImplementedException();
                        case OpType.MOD:
                            throw new NotImplementedException();
                        case OpType.LOG_NOT:
                            throw new NotImplementedException();
                        case OpType.LOG_AND:
                            throw new NotImplementedException();
                        case OpType.LOG_OR:
                            throw new NotImplementedException();
                        case OpType.BIT_NOT:
                            throw new NotImplementedException();
                        case OpType.BIT_AND:
                            throw new NotImplementedException();
                        case OpType.BIT_XOR:
                            throw new NotImplementedException();
                        case OpType.BIT_OR:
                            throw new NotImplementedException();
                        case OpType.BIT_SL:
                            throw new NotImplementedException();
                        case OpType.BIT_SR:
                            throw new NotImplementedException();
                        case OpType.EQ:
                            throw new NotImplementedException();
                        case OpType.NE:
                            throw new NotImplementedException();
                        case OpType.GE:
                            throw new NotImplementedException();
                        case OpType.GT:
                            throw new NotImplementedException();
                        case OpType.LE:
                            throw new NotImplementedException();
                        case OpType.LT:
                            throw new NotImplementedException();
                        case OpType.ASSIGN:
                            valueType = Expr2.CodeGen();
                            CodeGenPass.Constructor.AddDupT(valueType);   // Assign的返回值
                            Expr1.LeftValueCodeGen();
                            CodeGenPass.Constructor.AddStoreT(valueType);
                            return valueType;
                        case OpType.ADD_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SUB_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.MUL_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.DIV_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.MOD_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.AND_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.OR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.XOR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SL_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.CONDITIONAL:
                            throw new NotImplementedException();
                        case OpType.CAST:
                            throw new NotImplementedException();
                        case OpType.CALL:
                            CodeGenParam(Expr2);
                            valueType = Expr1.CodeGen();
                            CodeGenPass.Constructor.AddCall();
                            return ((FunctionType)valueType).ReturnType;
                        case OpType.CLASS_ACCESS:
                            throw new NotImplementedException();
                        case OpType.ARRAY_ACCESS:
                            throw new NotImplementedException();
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 因为参数要倒序入栈，所以用后序遍历的方式
        /// </summary>
        /// <param name="ps"></param>
        private void CodeGenParam(Expr ps)
        {
            if (ps == null)
            {
                return;
            }
            CodeGenParam((Expr)ps.SiblingAST);
            ps.CodeGen();
        }

        /// <summary>
        /// 将表达式的结果的左值入栈
        /// 栈顶是一个Address，所以不需要返回值来区分
        /// </summary>
        private void LeftValueCodeGen()
        {
            switch (ExprType)
            {
                case ExprType.ID:
                    if (CodeGenPass.Constructor.SymbolTable.TryGetValue(Value.StringValue, out Symbol symbol, out uint levelDiff))
                    {
                        if (symbol is FunctionSymbol function)
                        {
                            CodeGenPass.Constructor.AddPushA(function.Function.Index);
                        }
                        else if (symbol is VariableSymbol variable)
                        {
                            CodeGenPass.Constructor.AddGetA(levelDiff, variable.XirVariable.Offset);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new XiLangError($"Variable {Value.StringValue} is not declared when used", Line);
                    }
                    break;
                case ExprType.OPEXPR:
                    switch (OpType)
                    {
                        case OpType.ASSIGN:
                            throw new NotImplementedException();
                        case OpType.ADD_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SUB_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.MUL_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.DIV_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.MOD_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.AND_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.OR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.XOR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SL_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.SR_ASSIGN:
                            throw new NotImplementedException();
                        case OpType.CONDITIONAL:
                            throw new NotImplementedException();
                        case OpType.CAST:
                            throw new NotImplementedException();
                        case OpType.CALL:
                            throw new NotImplementedException();
                        case OpType.CLASS_ACCESS:
                            throw new NotImplementedException();
                        case OpType.ARRAY_ACCESS:
                            throw new NotImplementedException();
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
