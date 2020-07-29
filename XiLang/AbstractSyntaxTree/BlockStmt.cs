using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public class BlockStmt : Stmt
    {
        public Stmt Child { private set; get; }

        public BlockStmt(Stmt child)
        {
            Child = child;
        }

        public override AST[] Children()
        {
            return new AST[] { Child };
        }

        public override string ASTLabel()
        {
            return "(Block)";
        }

        public override XirValue CodeGen()
        {
            XirGenPass.VariableSymbolTable.Push();
            CodeGen(Child);
            XirGenPass.VariableSymbolTable.Pop();
            return null;
        }
    }
}
