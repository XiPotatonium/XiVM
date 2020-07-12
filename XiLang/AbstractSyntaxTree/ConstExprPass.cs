using System;
using System.Collections.Generic;
using System.Text;
using XiLang.PassMgr;

namespace XiLang.AbstractSyntaxTree
{
    class ConstExprPass : IASTPass
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

            foreach (var child in ast.Children())
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
