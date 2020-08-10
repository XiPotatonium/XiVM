using XiLang.Errors;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public enum JumpType
    {
        CONTINUE, BREAK, RETURN
    }

    internal class JumpStmt : Stmt
    {
        public JumpType Type { set; get; }
        public Expr ReturnVal { set; get; }

        public override AST[] Children()
        {
            return new AST[] { ReturnVal };
        }

        public override string ASTLabel()
        {
            return Type switch
            {
                JumpType.CONTINUE => "continue",
                JumpType.BREAK => "break",
                JumpType.RETURN => "return",
                _ => "UNK",
            };
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            switch (Type)
            {
                case JumpType.CONTINUE:
                    pass.Constructor.AddJmp(pass.Continuable.Peek());
                    break;
                case JumpType.BREAK:
                    pass.Constructor.AddJmp(pass.Breakable.Peek());
                    break;
                case JumpType.RETURN:
                    if (ReturnVal != null)
                    {
                        VariableType actualReturnType = ReturnVal.CodeGen(pass);
                        VariableType returnType = pass.Constructor.CurrentMethod.Type.ReturnType;
                        if (!returnType.Equivalent(actualReturnType))
                        {
                            throw new TypeError($"Expect return type {returnType}, actual return type {actualReturnType}", -1);
                        }
                    }
                    pass.Constructor.AddRet();
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
