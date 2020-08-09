using XiLang.Errors;
using XiLang.Pass;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public enum JumpType
    {
        CONTINUE, BREAK, RETURN
    }

    public class JumpStmt : Stmt
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

        public override VariableType CodeGen()
        {
            switch (Type)
            {
                case JumpType.CONTINUE:
                    Constructor.AddJmp(CodeGenPass.Continuable.Peek());
                    break;
                case JumpType.BREAK:
                    Constructor.AddJmp(CodeGenPass.Breakable.Peek());
                    break;
                case JumpType.RETURN:
                    if (ReturnVal != null)
                    {
                        VariableType actualReturnType = ReturnVal.CodeGen();
                        VariableType returnType = Constructor.CurrentMethod.Type.ReturnType;
                        if (!returnType.Equivalent(actualReturnType))
                        {
                            throw new TypeError($"Expect return type {returnType}, actual return type {actualReturnType}", -1);
                        }
                    }
                    Constructor.AddRet();
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
