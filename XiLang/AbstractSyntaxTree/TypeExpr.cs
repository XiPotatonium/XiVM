using System.Runtime.InteropServices;
using XiLang.Exceptions;
using XiLang.Lexical;

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
                case TokenType.FLOAT:
                    ret.Type = SyntacticValueType.FLOAT;
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
                    throw new SyntaxException("Unknown type", t);
            }
            return ret;
        }

        public SyntacticValueType Type { private set; get; }
        public bool IsArray { set; get; }
        public string ClassName { private set; get; }

        protected override string JsonName()
        {
            return "<" + Type switch
            {
                SyntacticValueType.BOOL => "bool",
                SyntacticValueType.INT => "int",
                SyntacticValueType.FLOAT => "float",
                SyntacticValueType.STRING => "string",
                SyntacticValueType.CLASS => ClassName,
                SyntacticValueType.VOID => "void",
                _ => "UNK",
            } + (IsArray ? "[]>" : ">");
        }

        protected override AST[] JsonChildren()
        {
            return new AST[0];
        }
    }
}
