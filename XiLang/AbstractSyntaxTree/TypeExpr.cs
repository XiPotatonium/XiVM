using XiLang.Exceptions;
using XiLang.Lexical;

namespace XiLang.AbstractSyntaxTree
{
    public class TypeExpr : Expr
    {
        public static TypeExpr FromToken(Token t)
        {
            TypeExpr ret = new TypeExpr();
            switch (t.Type)
            {
                case TokenType.BOOL:
                    ret.Type = BasicVarType.BOOL;
                    break;
                case TokenType.I32:
                    ret.Type = BasicVarType.I32;
                    break;
                case TokenType.F32:
                    ret.Type = BasicVarType.F32;
                    break;
                case TokenType.VOID:
                    ret.Type = BasicVarType.VOID;
                    break;
                case TokenType.STRING:
                    ret.Type = BasicVarType.STRING;
                    break;
                case TokenType.ID:
                    ret.Type = BasicVarType.USER_DEF;
                    ret.UserDefTypeName = t.Literal;
                    break;
                default:
                    throw new SyntaxException("Unknown var type", t);
            }
            return ret;
        }

        public BasicVarType Type { private set; get; }
        public bool IsArray { set; get; }
        public string UserDefTypeName { private set; get; }

        protected override string JsonName()
        {
            return "<" + Type switch
            {
                BasicVarType.BOOL => "bool",
                BasicVarType.I32 => "i32",
                BasicVarType.F32 => "f32",
                BasicVarType.VOID => "void",
                BasicVarType.STRING => "string",
                BasicVarType.USER_DEF => UserDefTypeName,
                _ => "UNK",
            } + (IsArray ? "[]>" : ">");
        }

        protected override AST[] JsonChildren()
        {
            return new AST[0];
        }
    }
}
