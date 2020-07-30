﻿using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class BlockStmt : Stmt
    {
        public Stmt Child { private set; get; }

        public BlockStmt(Stmt child)
        {
            Child = child;
        }

        public override AST[] Children()
        {
            return new AST[] { Child };
        }

        public override string ASTLabel()
        {
            return "(Block)";
        }

        public override VariableType CodeGen()
        {
            CodeGenPass.Constructor.SymbolTable.Push();
            CodeGen(Child);
            CodeGenPass.Constructor.SymbolTable.Pop();
            return null;
        }
    }
}
