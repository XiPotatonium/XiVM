namespace XiLang.AbstractSyntaxTree
{
    public class BlockStmt : Stmt
    {
        public Stmt Child { private set; get; }

        public BlockStmt(Stmt child)
        {
            Child = child;
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Child };
        }

        protected override string JsonName()
        {
            return "(Block)";
        }
    }
}
