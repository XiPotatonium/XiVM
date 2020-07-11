namespace XiLang.AbstractSyntaxTree
{
    public class ExprStmt : Stmt
    {
        public Expr Expr { private set; get; }

        public ExprStmt(Expr expr)
        {
            Expr = expr;
        }

        protected override string JsonName()
        {
            return "(ExprStmt)";
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Expr };
        }
    }
}
