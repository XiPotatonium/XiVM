﻿using System;
using System.Data;
using System.Reflection.Emit;
using XiLang.Errors;

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
                Value = XiLangValue.MakeFloat(literal)
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
                    StringVal = id
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

        public int Line { private set; get; }
        public ExprType ExprType { private set; get; }
        public OpType OpType { set; get; }
        public Expr Expr1 { set; get; }
        public Expr Expr2 { set; get; }
        public Expr Expr3 { set; get; }

        public XiLangValue Value { set; get; }

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
                switch (OpType)
                {
                    case OpType.NEG:
                        Value = Expr1.Value;
                        if (Value.Type == ValueType.INT)
                        {
                            Value.IntVal = -Value.IntVal;
                        }
                        else if (Value.Type == ValueType.FLOAT)
                        {
                            Value.FloatVal = -Value.FloatVal;
                        }
                        else
                        {
                            throw TypeError.ExpectType(OpType.ToString(), Value.Type, Line, ValueType.INT, ValueType.FLOAT);
                        }
                        break;
                    case OpType.ADD:
                    case OpType.SUB:
                    case OpType.MUL:
                    case OpType.DIV:
                    case OpType.MOD:
                        if (Expr1.Value.Type == ValueType.FLOAT || Expr2.Value.Type == ValueType.FLOAT)
                        {
                            Value = XiLangValue.Cast(ValueType.FLOAT, Expr1.Value, Expr1.Line);
                            Expr2.Value = XiLangValue.Cast(ValueType.FLOAT, Expr2.Value, Expr2.Line);
                            Value.FloatVal = OpType switch
                            {
                                OpType.ADD => Value.FloatVal + Expr2.Value.FloatVal,
                                OpType.SUB => Value.FloatVal - Expr2.Value.FloatVal,
                                OpType.MUL => Value.FloatVal * Expr2.Value.FloatVal,
                                OpType.DIV => Value.FloatVal / Expr2.Value.FloatVal,
                                OpType.MOD => Value.FloatVal % Expr2.Value.FloatVal,
                                _ => throw new NotImplementedException(),
                            };
                        }
                        else if (Expr1.Value.Type == ValueType.INT && Expr2.Value.Type == ValueType.INT)
                        {
                            Value = Expr1.Value;
                            Value.IntVal = OpType switch
                            {
                                OpType.ADD => Value.IntVal + Expr2.Value.IntVal,
                                OpType.SUB => Value.IntVal - Expr2.Value.IntVal,
                                OpType.MUL => Value.IntVal * Expr2.Value.IntVal,
                                OpType.DIV => Value.IntVal / Expr2.Value.IntVal,
                                OpType.MOD => Value.IntVal % Expr2.Value.IntVal,
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
                            Value = XiLangValue.MakeBool(!Expr1.Value.BoolVal);
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
                            OpType.LOG_AND => Expr1.Value.BoolVal && Expr2.Value.BoolVal,
                            OpType.LOG_OR => Expr1.Value.BoolVal || Expr2.Value.BoolVal,
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
                            Value.IntVal = ~Value.IntVal;
                        }
                        else
                        {   // 二元bit运算
                            Expr2.AssertValueType(OpType.ToString(), ValueType.INT);
                            Value.IntVal = OpType switch
                            {
                                OpType.BIT_AND => Value.IntVal & Expr2.Value.IntVal,
                                OpType.BIT_XOR => Value.IntVal & Expr2.Value.IntVal,
                                OpType.BIT_OR => Value.IntVal | Expr2.Value.IntVal,
                                OpType.BIT_SL => Value.IntVal << Expr2.Value.IntVal,
                                OpType.BIT_SR => Value.IntVal >> Expr2.Value.IntVal,
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
                        if (Expr1.Value.Type == ValueType.FLOAT || Expr2.Value.Type == ValueType.FLOAT)
                        {
                            Expr1.Value = XiLangValue.Cast(ValueType.FLOAT, Expr1.Value, Expr1.Line);
                            Expr2.Value = XiLangValue.Cast(ValueType.FLOAT, Expr2.Value, Expr2.Line);
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => Expr1.Value.FloatVal == Expr2.Value.FloatVal,
                                OpType.NE => Expr1.Value.FloatVal != Expr2.Value.FloatVal,
                                OpType.GE => Expr1.Value.FloatVal >= Expr2.Value.FloatVal,
                                OpType.GT => Expr1.Value.FloatVal > Expr2.Value.FloatVal,
                                OpType.LE => Expr1.Value.FloatVal <= Expr2.Value.FloatVal,
                                OpType.LT => Expr1.Value.FloatVal < Expr2.Value.FloatVal,
                                _ => throw new NotImplementedException(),
                            });
                        }
                        else if (Expr1.Value.Type == ValueType.INT && Expr2.Value.Type == ValueType.INT)
                        {
                            Value = XiLangValue.MakeBool(OpType switch
                            {
                                OpType.EQ => Expr1.Value.IntVal == Expr2.Value.IntVal,
                                OpType.NE => Expr1.Value.IntVal != Expr2.Value.IntVal,
                                OpType.GE => Expr1.Value.IntVal >= Expr2.Value.IntVal,
                                OpType.GT => Expr1.Value.IntVal > Expr2.Value.IntVal,
                                OpType.LE => Expr1.Value.IntVal <= Expr2.Value.IntVal,
                                OpType.LT => Expr1.Value.IntVal < Expr2.Value.IntVal,
                                _ => throw new NotImplementedException(),
                            });
                        }
                        else if (Expr1.Value.Type == ValueType.STRING && Expr2.Value.Type == ValueType.STRING)
                        {
                            int res = string.Compare(Expr1.Value.StringVal, Expr2.Value.StringVal);
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
                                OpType.EQ => Expr1.Value.BoolVal == Expr2.Value.BoolVal,
                                OpType.NE => Expr1.Value.BoolVal != Expr2.Value.BoolVal,
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
                            SyntacticValueType.FLOAT => ValueType.FLOAT,
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

        protected override string JsonName()
        {
            return ExprType switch
            {
                ExprType.CONST => Value.Type switch
                {
                    ValueType.INT => Value.IntVal.ToString(),
                    ValueType.FLOAT => Value.FloatVal.ToString(),
                    ValueType.STRING => $"\\\"{Value.StringVal}\\\"",
                    ValueType.BOOL => Value.IntVal == 0 ? "false" : "true",
                    ValueType.NULL => "null",
                    _ => "UNK",
                },
                ExprType.ID => $"(Id){Value.StringVal}",
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

        protected override AST[] JsonChildren()
        {
            return new AST[] { Expr1, Expr2, Expr3 };
        }
    }
}
