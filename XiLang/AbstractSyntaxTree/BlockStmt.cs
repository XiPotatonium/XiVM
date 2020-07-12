﻿namespace XiLang.AbstractSyntaxTree
{
    public class BlockStmt : Stmt
    {
        public Stmt Child { private set; get; }

        public BlockStmt(Stmt child)
        {
            Child = child;
        }

        public override AST[] JsonChildren()
        {
            return new AST[] { Child };
        }

        public override string JsonName()
        {
            return "(Block)";
        }
    }
}
