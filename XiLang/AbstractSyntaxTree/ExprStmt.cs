﻿using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal class ExprStmt : Stmt
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

        public override VariableType CodeGen(CodeGenPass pass)
        {
            AST ast = Expr;
            while (ast != null)
            {
                VariableType type = ast.CodeGen(pass);
                if (type != null)
                {
                    // 表达式的值依然在栈中，要pop出去
                    pass.Constructor.AddPop(type);
                }
                ast = ast.SiblingAST;
            }
            return null;
        }
    }
}
