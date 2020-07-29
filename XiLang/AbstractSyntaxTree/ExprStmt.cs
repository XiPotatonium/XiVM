using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public class ExprStmt : Stmt
    {
        public Expr Expr { private set; get; }

        public ExprStmt(Expr expr)
        {
            Expr = expr;
        }

        public override string ASTLabel()
        {
            return "(ExprStmt)";
        }

        public override AST[] Children()
        {
            return new AST[] { Expr };
        }

        public override XirValue CodeGen()
        {
            throw new System.NotImplementedException();
        }
    }
}
