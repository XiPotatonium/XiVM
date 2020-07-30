using XiVM;

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

        public override VariableType CodeGen()
        {
            AST ast = Expr;
            while (ast != null)
            {
                VariableType type = ast.CodeGen();
                // 表达式的值依然在栈中，要pop出去
                CodeGenPass.Constructor.AddPopValue(type);
                ast = ast.SiblingAST;
            }
            return null;
        }
    }
}
