namespace XiLang.AbstractSyntaxTree
{
    public class ExprStmt : Stmt
    {
        public Expr Expr { private set; get; }

        public ExprStmt(Expr expr)
        {
            Expr = expr;
        }

        public override string JsonName()
        {
            return "(ExprStmt)";
        }

        public override AST[] JsonChildren()
        {
            return new AST[] { Expr };
        }
    }
}
