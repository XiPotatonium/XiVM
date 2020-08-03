using System;
using XiLang.Errors;
using XiLang.Lexical;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class TypeExpr : Expr
    {
        /// <summary>
        /// 注意void不是Type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static TypeExpr FromToken(Token t)
        {
            TypeExpr ret = new TypeExpr();
            switch (t.Type)
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
                    ret.ClassName = t.Literal;
                    break;
                default:
                    throw new SyntaxError("Unknown type", t);
            }
            return ret;
        }

        public SyntacticValueType Type { private set; get; }
        public bool IsArray { set; get; }
        public string ClassName { private set; get; }

        public override string ASTLabel()
        {
            return "<" + Type switch
            {
                SyntacticValueType.BOOL => "bool",
                SyntacticValueType.INT => "int",
                SyntacticValueType.DOUBLE => "float",
                SyntacticValueType.STRING => "string",
                SyntacticValueType.CLASS => ClassName,
                SyntacticValueType.VOID => "void",
                _ => "UNK",
            } + (IsArray ? "[]>" : ">");
        }

        /// <summary>
        /// Type表达式是一个常量，在设计中不允许有child
        /// </summary>
        /// <returns></returns>
        public override AST[] Children()
        {
            return new AST[0];
        }

        public VariableType ToXirType()
        {
            if (IsArray)
            {
                return Type switch
                {
                    SyntacticValueType.BOOL => Constructor.ByteArrayType,
                    SyntacticValueType.INT => throw new NotImplementedException(),
                    SyntacticValueType.DOUBLE => throw new NotImplementedException(),
                    SyntacticValueType.STRING => Constructor.AddressArrayType,
                    SyntacticValueType.CLASS => Constructor.AddressArrayType,
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                return Type switch
                {
                    SyntacticValueType.BOOL => VariableType.ByteType,
                    SyntacticValueType.INT => VariableType.IntType,
                    SyntacticValueType.DOUBLE => VariableType.DoubleType,
                    SyntacticValueType.STRING => Constructor.StringType,
                    SyntacticValueType.CLASS => throw new NotImplementedException(),
                    SyntacticValueType.VOID => null,
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }
}
