using XiLang.AbstractSyntaxTree;
using XiLang.PassMgr;

namespace XiLang
{
    internal class ConstExprPass : IASTPass
    {
        public object Run(AST root)
        {
            EvaluateConstExpr(root);
            return root;
        }

        private void EvaluateConstExpr(AST ast)
        {
            if (ast == null)
            {
                return;
            }

            if (ast is Expr expr)
            {
                if (expr.IsConst())
                {
                    expr.Value = expr.EvaluateConstExpr();
                    expr.ExprType = ExprType.CONST;
                    expr.Expr1 = expr.Expr2 = expr.Expr3 = null;
                    return;
                }
            }

            foreach (AST child in ast.Children())
            {
                EvaluateConstExpr(child);
            }

            if (ast.SiblingAST != null)
            {
                EvaluateConstExpr(ast.SiblingAST);
            }
        }
    }
}
