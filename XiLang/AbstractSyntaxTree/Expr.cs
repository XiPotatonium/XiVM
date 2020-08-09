using System;
using System.Collections.Generic;
using System.Linq;
using XiLang.Errors;
using XiLang.Pass;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class Expr : AST
    {
        #region Static Make

        public static Expr MakeOp(OpType type, Expr lhs, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = lhs
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs, Expr rhs, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = lhs,
                Expr2 = rhs
            };
        }

        public static Expr MakeOp(OpType type, Expr expr1, Expr expr2, Expr expr3, int line)
        {
            return new Expr(line)
            {
                OpType = type,
                Expr1 = expr1,
                Expr2 = expr2,
                Expr3 = expr3
            };
        }

        #endregion

        public int Line { private set; get; }
        public OpType OpType { set; get; }
        public Expr Expr1 { set; get; }
        public Expr Expr2 { set; get; }
        public Expr Expr3 { set; get; }

        protected Expr() { }
        protected Expr(int line)
        {
            Line = line;
        }

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

        public override string ASTLabel()
        {
            return OpType switch
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
                _ => throw new NotImplementedException(),
            };
        }

        public override AST[] Children()
        {
            return new AST[] { Expr1, Expr2, Expr3 };
        }

        public override VariableType CodeGen()
        {
            VariableType expr1Type, expr2Type;
            string moduleName, className, memberName;

            if (IsConst())
            {
                // 常量表达式，伪造一个常量子节点
                Expr1 = new ConstExpr(Line)
                {
                    Value = EvaluateConstExpr()
                };
                return Expr1.CodeGen();
            }

            switch (OpType)
            {
                case OpType.NEG:
                    expr1Type = Expr1.CodeGen();
                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return Constructor.AddNegI();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case OpType.INC:
                    throw new NotImplementedException();
                case OpType.DEC:
                    throw new NotImplementedException();
                case OpType.ADD:
                case OpType.SUB:
                case OpType.MUL:
                case OpType.DIV:
                    expr1Type = Expr1.CodeGen();
                    expr2Type = Expr2.CodeGen();
                    if (!expr1Type.Equivalent(expr2Type))
                    {
                        throw new TypeError($"{OpType}: {expr1Type} and {expr2Type} is not equivalent", Line);
                    }

                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return OpType switch
                        {
                            OpType.ADD => Constructor.AddAddI(),
                            OpType.SUB => Constructor.AddSubI(),
                            OpType.MUL => Constructor.AddMulI(),
                            OpType.DIV => Constructor.AddDivI(),
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                case OpType.MOD:
                    expr1Type = Expr1.CodeGen();
                    if (!expr1Type.Equivalent(VariableType.IntType))
                    {
                        throw new TypeError($"{OpType} expect int type", Line);
                    }
                    expr2Type = Expr2.CodeGen();
                    if (!expr2Type.Equivalent(VariableType.IntType))
                    {
                        throw new TypeError($"{OpType} expect int type", Line);
                    }
                    return Constructor.AddMod();
                case OpType.LOG_NOT:
                    throw new NotImplementedException();
                case OpType.LOG_AND:
                    throw new NotImplementedException();
                case OpType.LOG_OR:
                    throw new NotImplementedException();
                case OpType.BIT_NOT:
                case OpType.BIT_AND:
                case OpType.BIT_XOR:
                case OpType.BIT_OR:
                case OpType.BIT_SL:
                case OpType.BIT_SR:
                    // 位操作不在计划之中
                    throw new NotImplementedException();
                case OpType.EQ:
                case OpType.NE:
                case OpType.GE:
                case OpType.GT:
                case OpType.LE:
                case OpType.LT:
                    expr1Type = Expr1.CodeGen();
                    expr2Type = Expr2.CodeGen();
                    if (!expr1Type.Equivalent(expr2Type))
                    {
                        throw new TypeError($"{OpType}: {expr1Type} and {expr2Type} is not equivalent", Line);
                    }

                    if (expr1Type.Tag == VariableTypeTag.INT)
                    {
                        return OpType switch
                        {
                            OpType.EQ => Constructor.AddSetEqI(),
                            OpType.NE => Constructor.AddSetNeI(),
                            OpType.GE => Constructor.AddSetGeI(),
                            OpType.GT => Constructor.AddSetGtI(),
                            OpType.LE => Constructor.AddSetLeI(),
                            OpType.LT => Constructor.AddSetLtI(),
                            _ => throw new NotImplementedException(),
                        };
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    throw new NotImplementedException();
                case OpType.ASSIGN:
                    expr2Type = Expr2.CodeGen();
                    Constructor.AddDup(expr2Type);   // Assign的返回值
                    Expr1.LeftValueCodeGen();
                    Constructor.AddStoreT(expr2Type);
                    return expr2Type;
                case OpType.ADD_ASSIGN:
                case OpType.SUB_ASSIGN:
                case OpType.MUL_ASSIGN:
                case OpType.DIV_ASSIGN:
                case OpType.MOD_ASSIGN:
                    // 特殊assign优先级低
                    throw new NotImplementedException();
                case OpType.AND_ASSIGN:
                case OpType.OR_ASSIGN:
                case OpType.XOR_ASSIGN:
                case OpType.SL_ASSIGN:
                case OpType.SR_ASSIGN:
                    // 位操作不在计划之中
                    throw new NotImplementedException();
                case OpType.CONDITIONAL:
                    throw new NotImplementedException();
                case OpType.CAST:
                    //expr1Type = ((TypeExpr)Expr1).ToXirType();
                    //expr2Type = Expr2.CodeGen();
                    throw new NotImplementedException();
                case OpType.CALL:
                    List<Expr> ps = new List<Expr>();
                    Expr3 = Expr2;  // 借用Expr3作临时变量
                    while (Expr3 != null)
                    {
                        ps.Add(Expr3);
                        Expr3 = (Expr)Expr3.SiblingAST;
                    }
                    Expr3 = null;

                    // 查找潜在函数集
                    (moduleName, className, memberName) = GetFullName(Expr1);
                    if (!Program.TryGetMethod(moduleName, className, memberName, out List<MethodType> candidateMethods))
                    {
                        throw new XiLangError($"{moduleName}.{className}.{memberName} not found");
                    }
                    candidateMethods = candidateMethods.Where(m => m.Params.Count == ps.Count).ToList();

                    // 参数倒序进栈
                    List<VariableType> pTypes = new List<VariableType>();   // 正序
                    for (int i = ps.Count - 1; i >= 0; --i)
                    {
                        pTypes.Add(ps[i].CodeGen());
                    }

                    // 确定对应函数
                    MethodType methodType = null;
                    foreach (MethodType candidate in candidateMethods)
                    {
                        bool flag = true;
                        foreach ((VariableType realType, VariableType actualType) in candidate.Params.Zip(pTypes))
                        {
                            if (!realType.Equivalent(actualType))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            methodType = candidate;
                        }
                    }

                    if (methodType == null)
                    {
                        throw new XiLangError($"No matched method");
                    }

                    Constructor.AddCall(Constructor.AddMethodPoolInfo(
                        Constructor.AddClassPoolInfo(moduleName, className),
                        memberName, methodType.ToString()));
                    return methodType.ReturnType;
                case OpType.CLASS_ACCESS:
                    throw new NotImplementedException();
                case OpType.ARRAY_ACCESS:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 负责获取全名，即使那个表达式只有部分全名，也要补全
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private static (string, string, string) GetFullName(Expr expr)
        {
            string moduleName, className, memberName;
            if (expr is IdExpr)
            {
                memberName = ((IdExpr)expr).Id;
                className = Constructor.CurrentClass.Name;
                moduleName = Constructor.Module.Name;
            }
            else if (expr.OpType == OpType.CLASS_ACCESS)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            return (moduleName, className, memberName);
        }

        /// <summary>
        /// 期望获得一个左值
        /// 将表达式的结果的地址入栈
        /// 栈顶是一个Address，所以不需要返回值来区分
        /// </summary>
        protected virtual void LeftValueCodeGen()
        {
            switch (OpType)
            {
                case OpType.CLASS_ACCESS:
                    throw new NotImplementedException();
                case OpType.ARRAY_ACCESS:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class ConstExpr : Expr
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

        public override VariableType CodeGen()
        {
            switch (Value.Type)
            {
                case ValueType.INT:
                    Constructor.AddPushI(Value.IntValue);
                    return VariableType.IntType;
                case ValueType.DOUBLE:
                    Constructor.AddPushD(Value.DoubleValue);
                    return VariableType.DoubleType;
                case ValueType.STRING:
                    Constructor.AddConst(Constructor.StringPool.TryAdd(Value.StringValue));
                    return Program.StringType;
                case ValueType.BOOL:
                    Constructor.AddPushB(Value.BoolValue ? (byte)1 : (byte)0);
                    return VariableType.ByteType;
                case ValueType.NULL:
                    Constructor.AddPushA(0);
                    return VariableType.AddressType;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class IdExpr : Expr
    {
        public static IdExpr MakeId(string id, int line)
        {
            return new IdExpr(id, line);
        }

        public string Id { private set; get; }

        public IdExpr(string id, int line)
            : base(line)
        {
            Id = id;
        }

        public override bool IsConst()
        {
            return false;
        }

        public override string ASTLabel()
        {
            return $"(Id){Id}";
        }

        public override VariableType CodeGen()
        {
            if (CodeGenPass.LocalSymbolTable.TryGetSymbol(Id, out Variable variable))
            {
                // 是一个局部变量
                Constructor.AddLocal(variable.Offset);
                Constructor.AddLoadT(variable.Type);
                return variable.Type;
            }
            else if (Constructor.CurrentClass.Fields.TryGetValue(Id, out ClassField field))
            {
                // static或者非static field
                Constructor.AddGetStaticFieldAddress(field);
                Constructor.AddLoadT(field.Type);
                return field.Type;
            }
            else
            {
                // 暂时不支持函数赋值
                throw new NotImplementedException();
            }
        }

        protected override void LeftValueCodeGen()
        {
            if (CodeGenPass.LocalSymbolTable.TryGetSymbol(Id, out Variable variable))
            {
                // 是一个局部变量
                Constructor.AddLocal(variable.Offset);
            }
            else if (Constructor.CurrentClass.Fields.TryGetValue(Id, out ClassField field))
            {
                // static或者非static field
                Constructor.AddGetStaticFieldAddress(field);
            }
            else
            {
                // 暂时不支持函数赋值
                throw new NotImplementedException();
            }
        }
    }
}
