using System;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Lexical;

namespace XiLang.Syntactic
{
    public partial class Parser
    {
        /// <summary>
        /// TypeExpr
        ///     ANY_TYPE_MODIFIER* (ANY_TYPE | ID) (LBRACKET RBRACKET)?
        /// </summary>
        /// <returns></returns>
        private TypeExpr ParseTypeExpr()
        {
            TypeExpr ret = new TypeExpr();
            Token typeToken;

            while (!Check(LexicalRules.TypeTokens) && !Check(TokenType.ID))
            {
                // 解析Attribute
                if (Check(TokenType.STATIC))
                {
                    typeToken = Consume(TokenType.STATIC);
                    if ((ret.Modifier & (uint)TypeModifier.STATIC) != 0)
                    {
                        throw new SyntaxError("Duplicated static modifier", typeToken);
                    }
                    ret.Modifier |= (uint)TypeModifier.STATIC;
                }
            }

            typeToken = Consume();
            switch (typeToken.Type)
            {
                case TokenType.BOOL:
                    ret.Type = SyntacticValueType.BOOL;
                    break;
                case TokenType.INT:
                    ret.Type = SyntacticValueType.INT;
                    break;
                case TokenType.DOUBLE:
                    ret.Type = SyntacticValueType.DOUBLE;
                    break;
                case TokenType.VOID:
                    ret.Type = SyntacticValueType.VOID;
                    break;
                case TokenType.STRING:
                    ret.Type = SyntacticValueType.STRING;
                    break;
                case TokenType.ID:
                    ret.Type = SyntacticValueType.CLASS;
                    ret.ClassName = typeToken.Literal;
                    break;
                default:
                    throw new SyntaxError("Unknown type", typeToken);
            }

            if (Check(TokenType.LBRACKET))
            {
                Consume(TokenType.LBRACKET);
                Consume(TokenType.RBRACKET);
                ret.IsArray = true;
            }
            return ret;
        }

        /// <summary>
        /// ListExpr
        ///     Expr [COMMA Expr]*
        /// </summary>
        /// <returns></returns>
        private Expr ParseExprList()
        {
            Expr root = ParseExpr();
            Expr cur = root;
            while (Check(TokenType.COMMA))
            {
                Consume(TokenType.COMMA);
                AppendASTLinkedList(ref root, ref cur, ParseExpr());
            }
            return root;
        }

        /// <summary>
        /// 就是AssignmentExpr
        /// Expr
        ///     (ConditionalExpr | Id) (ANY_ASSIGN Expr)?
        /// </summary>
        /// <param name="isDecl">true表示decl，lhs只能是id不能是表达式</param>
        /// <returns></returns>
        private Expr ParseExpr(bool isDecl = false)
        {
            Expr lhs = isDecl ? ParseId() : ParseCondtionalExpr();
            if (Check(TokenType.ASSIGN, TokenType.ADD_ASSIGN, TokenType.SUB_ASSIGN, TokenType.MUL_ASSIGN, TokenType.DIV_ASSIGN,
                TokenType.MOD_ASSIGN, TokenType.OR_ASSIGN, TokenType.AND_ASSIGN, TokenType.SR_ASSIGN, TokenType.SL_ASSIGN))
            {
                Token t = Consume(TokenType.ASSIGN, TokenType.ADD_ASSIGN, TokenType.SUB_ASSIGN, TokenType.MUL_ASSIGN, TokenType.DIV_ASSIGN,
                    TokenType.MOD_ASSIGN, TokenType.OR_ASSIGN, TokenType.AND_ASSIGN, TokenType.SR_ASSIGN, TokenType.SL_ASSIGN);
                return t.Type switch
                {
                    TokenType.ASSIGN => Expr.MakeOp(OpType.ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.ADD_ASSIGN => Expr.MakeOp(OpType.ADD_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.SUB_ASSIGN => Expr.MakeOp(OpType.SUB_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.MUL_ASSIGN => Expr.MakeOp(OpType.MUL_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.DIV_ASSIGN => Expr.MakeOp(OpType.DIV_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.MOD_ASSIGN => Expr.MakeOp(OpType.MOD_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.AND_ASSIGN => Expr.MakeOp(OpType.AND_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.OR_ASSIGN => Expr.MakeOp(OpType.OR_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.XOR_ASSIGN => Expr.MakeOp(OpType.XOR_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.SL_ASSIGN => Expr.MakeOp(OpType.SL_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    TokenType.SR_ASSIGN => Expr.MakeOp(OpType.SR_ASSIGN, lhs, ParseExpr(), lhs.Line),
                    _ => null,// 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// ConditionalExpr
        ///     LogicalOrExpr (QUESTION Expr COLON ConditionalExpr)?
        /// </summary>
        /// <returns></returns>
        private Expr ParseCondtionalExpr()
        {
            Expr expr1 = ParseLogicalOrExpr();
            if (Check(TokenType.QUESTION))
            {
                Consume(TokenType.QUESTION);
                Expr expr2 = ParseExpr();
                Consume(TokenType.COLON);
                Expr expr3 = ParseCondtionalExpr();
                return Expr.MakeOp(OpType.CONDITIONAL, expr1, expr2, expr3, expr1.Line);
            }
            return expr1;
        }

        /// <summary>
        /// LogicalOrExpr
        ///     LogicalAndExpr (LOG_OR LogicalAndExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseLogicalOrExpr()
        {
            Expr lhs = ParseLogicalAndExpr();
            while (Check(TokenType.LOG_OR))
            {
                Consume(TokenType.LOG_OR);
                lhs = Expr.MakeOp(OpType.LOG_OR, lhs, ParseLogicalAndExpr(), lhs.Line);
            }
            return lhs;
        }

        /// <summary>
        /// LogicalAndExpr
        ///     BitOrExpr (LOG_AND BitOrExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseLogicalAndExpr()
        {
            Expr lhs = ParseBitOrExpr();
            while (Check(TokenType.LOG_AND))
            {
                Consume(TokenType.LOG_AND);
                lhs = Expr.MakeOp(OpType.LOG_AND, lhs, ParseBitOrExpr(), lhs.Line);
            }
            return lhs;
        }

        /// <summary>
        /// BitOrExpr
        ///     BitXorExpr (BIT_OR BitXorExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseBitOrExpr()
        {
            Expr lhs = ParseBitXorExpr();
            while (Check(TokenType.BIT_OR))
            {
                Consume(TokenType.BIT_OR);
                lhs = Expr.MakeOp(OpType.BIT_OR, lhs, ParseBitXorExpr(), lhs.Line);
            }
            return lhs;
        }

        /// <summary>
        /// BitXorExpr
        ///     BitAndExpr (BIT_XOR BitAndExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseBitXorExpr()
        {
            Expr lhs = ParseBitAndExpr();
            while (Check(TokenType.BIT_XOR))
            {
                Consume(TokenType.BIT_XOR);
                lhs = Expr.MakeOp(OpType.BIT_XOR, lhs, ParseBitAndExpr(), lhs.Line);
            }
            return lhs;
        }

        /// <summary>
        /// BitAndExpr
        ///     EqExpr (BIT_AND EqExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseBitAndExpr()
        {
            Expr lhs = ParseEqExpr();
            while (Check(TokenType.BIT_AND))
            {
                Consume(TokenType.BIT_AND);
                lhs = Expr.MakeOp(OpType.BIT_AND, lhs, ParseEqExpr(), lhs.Line);
            }
            return lhs;
        }

        /// <summary>
        /// EqExpr
        ///     CompExpr ((EQ | NE) CompExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseEqExpr()
        {
            Expr lhs = ParseCompExpr();
            while (Check(TokenType.EQ, TokenType.NE))
            {
                Token t = Consume(TokenType.EQ, TokenType.NE);
                lhs = t.Type switch
                {
                    TokenType.EQ => Expr.MakeOp(OpType.EQ, lhs, ParseCompExpr(), lhs.Line),
                    TokenType.NE => Expr.MakeOp(OpType.NE, lhs, ParseCompExpr(), lhs.Line),
                    _ => null   // 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// CompExpr
        ///     ShiftExpr ((LE | LT | GE | GT) ShiftExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseCompExpr()
        {
            Expr lhs = ParseShiftExpr();
            while (Check(TokenType.GT, TokenType.GE, TokenType.LT, TokenType.LE))
            {
                Token t = Consume(TokenType.GT, TokenType.GE, TokenType.LT, TokenType.LE);
                lhs = t.Type switch
                {
                    TokenType.GT => Expr.MakeOp(OpType.GT, lhs, ParseShiftExpr(), lhs.Line),
                    TokenType.GE => Expr.MakeOp(OpType.GE, lhs, ParseShiftExpr(), lhs.Line),
                    TokenType.LT => Expr.MakeOp(OpType.LT, lhs, ParseShiftExpr(), lhs.Line),
                    TokenType.LE => Expr.MakeOp(OpType.LE, lhs, ParseShiftExpr(), lhs.Line),
                    _ => null   // 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// ShiftExpr
        ///     AddExpr ((BIT_SL | BIT_SR) AddExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseShiftExpr()
        {
            Expr lhs = ParseAddExpr();
            while (Check(TokenType.BIT_SL, TokenType.BIT_SR))
            {
                Token t = Consume(TokenType.BIT_SL, TokenType.BIT_SR);
                lhs = t.Type switch
                {
                    TokenType.BIT_SL => Expr.MakeOp(OpType.BIT_SL, lhs, ParseAddExpr(), lhs.Line),
                    TokenType.BIT_SR => Expr.MakeOp(OpType.BIT_SR, lhs, ParseAddExpr(), lhs.Line),
                    _ => null   // 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// AddExpr
        ///     MulExpr ((ADD | SUB) MulExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseAddExpr()
        {
            Expr lhs = ParseMulExpr();
            while (Check(TokenType.ADD, TokenType.SUB))
            {
                Token t = Consume(TokenType.ADD, TokenType.SUB);
                lhs = t.Type switch
                {
                    TokenType.ADD => Expr.MakeOp(OpType.ADD, lhs, ParseMulExpr(), lhs.Line),
                    TokenType.SUB => Expr.MakeOp(OpType.SUB, lhs, ParseMulExpr(), lhs.Line),
                    _ => null   // 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// MulExpr
        ///     CastExpr ((DIV | MUL | MOD) CastExpr)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseMulExpr()
        {
            Expr lhs = ParseCastExpr();
            while (Check(TokenType.MUL, TokenType.DIV, TokenType.MOD))
            {
                Token t = Consume(TokenType.MUL, TokenType.DIV, TokenType.MOD);
                lhs = t.Type switch
                {
                    TokenType.MUL => Expr.MakeOp(OpType.MUL, lhs, ParseCastExpr(), lhs.Line),
                    TokenType.DIV => Expr.MakeOp(OpType.DIV, lhs, ParseCastExpr(), lhs.Line),
                    TokenType.MOD => Expr.MakeOp(OpType.MOD, lhs, ParseCastExpr(), lhs.Line),
                    _ => null   // 不会运行到这
                };
            }
            return lhs;
        }

        /// <summary>
        /// CastExpr
        ///     LPAREN TypeExpr RPAREN CastExpr
        ///     UnaryExpr
        /// </summary>
        /// <returns></returns>
        private Expr ParseCastExpr()
        {
            // 注意UnaryExpr中存在括号表达式
            // 需要解决Cast和括号表达式的冲突，因此需要判断是否是TypeExpr
            if (Check(TokenType.LPAREN) && IsTypeExprPrefix(1))
            {
                Consume(TokenType.LPAREN);
                Expr type = ParseTypeExpr();
                Consume(TokenType.RPAREN);
                return Expr.MakeOp(OpType.CAST, type, ParseCastExpr(), type.Line);
            }
            return ParseUnaryExpr();
        }

        /// <summary>
        /// UnaryExpr
        ///     (LOG_NOT | BIT_NOT | SUB) CastExpr | (INC | DEC) UnaryExpr | CallExpr
        /// </summary>
        /// <returns></returns>
        private Expr ParseUnaryExpr()
        {
            if (Check(TokenType.LOG_NOT, TokenType.BIT_NOT, TokenType.SUB))
            {
                Token t = Consume(TokenType.LOG_NOT, TokenType.BIT_NOT, TokenType.SUB);
                return t.Type switch
                {
                    TokenType.BIT_NOT => Expr.MakeOp(OpType.BIT_NOT, ParseCastExpr(), t.Line),
                    TokenType.LOG_NOT => Expr.MakeOp(OpType.LOG_NOT, ParseCastExpr(), t.Line),
                    TokenType.SUB => Expr.MakeOp(OpType.NEG, ParseCastExpr(), t.Line),
                    _ => null   // 不会运行到这
                };
            }
            else if (Check(TokenType.INC, TokenType.DEC))
            {
                Token t = Consume(TokenType.INC, TokenType.DEC);
                return t.Type switch
                {
                    TokenType.INC => Expr.MakeOp(OpType.INC, ParseUnaryExpr(), t.Line),
                    TokenType.DEC => Expr.MakeOp(OpType.DEC, ParseUnaryExpr(), t.Line),
                    _ => null   // 不会运行到这
                };
            }
            return ParseCallExpr();
        }

        /// <summary>
        /// CallExpr
        ///     PrimaryExpr (LPAREN (Expr (COMMA Expr)*)? RPAREN | DOT Id | LBRACKET Expr RBRACKET)*
        /// </summary>
        /// <returns></returns>
        private Expr ParseCallExpr()
        {
            Expr lhs = ParsePrimaryExpr();
            Expr ps = null;
            Expr cur = null;
            while (true)
            {
                if (Check(TokenType.LPAREN))
                {   // Call
                    Consume(TokenType.LPAREN);
                    if (!Check(TokenType.RPAREN))
                    {
                        while (true)
                        {
                            AppendASTLinkedList(ref ps, ref cur, ParseExpr());

                            if (Check(TokenType.COMMA))
                            {
                                Consume(TokenType.COMMA);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    Consume(TokenType.RPAREN);
                    lhs = Expr.MakeOp(OpType.CALL, lhs, ps, lhs.Line);
                }
                else if (Check(TokenType.DOT))
                {   // Access
                    Consume(TokenType.DOT);
                    lhs = Expr.MakeOp(OpType.CLASS_ACCESS, lhs, ParseId(), lhs.Line);
                }
                else if (Check(TokenType.LBRACKET))
                {   // 数组访问
                    Consume(TokenType.LBRACKET);
                    lhs = Expr.MakeOp(OpType.ARRAY_ACCESS, lhs, ParseExpr(), lhs.Line);
                    Consume(TokenType.RBRACKET);
                }
                else
                {
                    break;
                }
            }
            return lhs;
        }

        /// <summary>
        /// PrimaryExpr
        ///     ConstExpr | Id | LPAREN Expr RPAREN | BASE DOT Id
        /// </summary>
        /// <returns></returns>
        private Expr ParsePrimaryExpr()
        {
            if (Check(TokenType.ID))
            {
                return ParseId();
            }
            if (Check(TokenType.LPAREN))
            {
                Consume(TokenType.LPAREN);
                Expr ret = ParseExpr();
                Consume(TokenType.RPAREN);
                return ret;
            }
            if (Check(TokenType.BASE))
            {
                Consume(TokenType.BASE);
                Consume(TokenType.DOT);
                // TODO Base
                throw new NotImplementedException();
            }
            return ParseConstExpr();
        }

        /// <summary>
        /// Id
        ///     ID
        /// </summary>
        /// <returns></returns>
        private Expr ParseId()
        {
            Token t = Consume(TokenType.ID);
            return Expr.MakeId(t.Literal, t.Line);
        }

        /// <summary>
        /// ConstExpr
        ///     TRUE | FALSE | NULL | DEC_LITERAL | HEX_LITERAL | FLOAT_LITERAL | STR_LITERAL | CHAR_LITERAL
        /// </summary>
        /// <returns></returns>
        private Expr ParseConstExpr()
        {
            Token t = Consume(TokenType.TRUE, TokenType.FALSE, TokenType.NULL,
                            TokenType.DEC_LITERAL, TokenType.HEX_LITERAL, TokenType.FLOAT_LITERAL,
                            TokenType.STR_LITERAL, TokenType.CHAR_LITERAL);
            switch (t.Type)
            {
                case TokenType.NULL:
                    return Expr.MakeNull(t.Line);
                case TokenType.TRUE:
                    return Expr.MakeBool(true, t.Line);
                case TokenType.FALSE:
                    return Expr.MakeBool(false, t.Line);
                case TokenType.DEC_LITERAL:
                    return Expr.MakeInt(t.Literal, 10, t.Line);
                case TokenType.HEX_LITERAL:
                    return Expr.MakeInt(t.Literal, 16, t.Line);
                case TokenType.FLOAT_LITERAL:
                    return Expr.MakeFloat(t.Literal, t.Line);
                case TokenType.STR_LITERAL:
                    return Expr.MakeString(t.Literal, t.Line);
                case TokenType.CHAR_LITERAL:
                    return Expr.MakeChar(t.Literal, t.Line);
                default:
                    break;
            }
            return null;        // 不会运行到这里
        }
    }
}
