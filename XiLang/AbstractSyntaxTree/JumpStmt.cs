using XiVM.Xir;

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

        public override XirValue CodeGen()
        {
            switch (Type)
            {
                case JumpType.CONTINUE:
                    throw new System.NotImplementedException();
                case JumpType.BREAK:
                    throw new System.NotImplementedException();
                case JumpType.RETURN:
                    XirGenPass.ModuleConstructor.AddReturnInstruction(ReturnVal?.CodeGen());
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
