﻿namespace XiLang.AbstractSyntaxTree
{
    public class IfStmt : Stmt
    {
        public static IfStmt MakeIf(Expr cond, Stmt then, Stmt otherwise)
        {
            return new IfStmt()
            {
                Cond = cond,
                Then = then,
                Otherwise = otherwise
            };
        }

        public Expr Cond { private set; get; }
        public Stmt Then { private set; get; }
        public Stmt Otherwise { private set; get; }

        public override string JsonName()
        {
            return "(If)";
        }

        public override AST[] JsonChildren()
        {
            return new AST[] { Cond, Then, Otherwise };
        }
    }
}
