using System;

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
        public static Expr MakeNull()
        {
            return new Expr()
            {
                ExprType = ExprType.CONST,
                Value = XiLangValue.MakeNull()
            };
        }


        public static Expr MakeBool(bool val)
        {
            return new Expr()
            {
                ExprType = ExprType.CONST,
                Value = XiLangValue.MakeBool(val)
            };
        }

        public static Expr MakeInt(string literal, int fromBase = 10)
        {
            return new Expr()
            {
                ExprType = ExprType.CONST,
                Value = XiLangValue.MakeInt(literal, fromBase)
            };
        }

        public static Expr MakeFloat(string literal)
        {
            return new Expr()
            {
                ExprType = ExprType.CONST,
                Value = XiLangValue.MakeFloat(literal)
            };
        }

        public static Expr MakeString(string literal)
        {
            return new Expr()
            {
                ExprType = ExprType.CONST,
                Value = XiLangValue.MakeString(literal)
            };
        }

        /// <summary>
        /// 约定id值放在Value.StrVal里
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Expr MakeId(string id)
        {
            return new Expr()
            {
                ExprType = ExprType.ID,
                Value = new XiLangValue()
                {
                    StrVal = id
                }
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs)
        {
            return new Expr()
            {
                OpType = type,
                Expr1 = lhs
            };
        }

        public static Expr MakeOp(OpType type, Expr lhs, Expr rhs)
        {
            return new Expr()
            {
                OpType = type,
                Expr1 = lhs,
                Expr2 = rhs
            };
        }

        public static Expr MakeOp(OpType type, Expr expr1, Expr expr2, Expr expr3)
        {
            return new Expr()
            {
                OpType = type,
                Expr1 = expr1,
                Expr2 = expr2,
                Expr3 = expr3
            };
        }

        public ExprType ExprType { set; get; } = ExprType.OPEXPR;
        public OpType OpType { set; get; }
        public Expr Expr1 { set; get; }
        public Expr Expr2 { set; get; }
        public Expr Expr3 { set; get; }

        public XiLangValue Value { set; get; }

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

        public XiLangValue Evaluate()
        {
            throw new NotImplementedException();
        }

        protected override string JsonName()
        {
            return ExprType switch
            {
                ExprType.CONST => Value.Type switch
                {
                    ValueType.INT => Value.IntVal.ToString(),
                    ValueType.FLOAT => Value.FloatVal.ToString(),
                    ValueType.STR => $"\\\"{Value.StrVal}\\\"",
                    ValueType.BOOL => Value.IntVal == 0 ? "false" : "true",
                    ValueType.NULL => "null",
                    _ => "UNK",
                },
                ExprType.ID => $"(Id){Value.StrVal}",
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
