using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal class BlockStmt : Stmt
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

        public override VariableType CodeGen(CodeGenPass pass)
        {
            pass.LocalSymbolTable.PushFrame();
            CodeGen(pass, Child);
            pass.LocalSymbolTable.PopFrame();
            return null;
        }
    }
}
