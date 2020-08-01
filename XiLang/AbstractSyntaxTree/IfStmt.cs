﻿using System.Collections.Generic;
using XiLang.Pass;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public class IfStmt : Stmt
    {
        public static IfStmt MakeIf(Expr cond, BlockStmt then, Stmt otherwise)
        {
            return new IfStmt()
            {
                Cond = cond,
                Then = then,
                Otherwise = otherwise
            };
        }

        public Expr Cond { private set; get; }
        public BlockStmt Then { private set; get; }
        public Stmt Otherwise { private set; get; }

        public override string ASTLabel()
        {
            return "(If)";
        }

        public override AST[] Children()
        {
            return new AST[] { Cond, Then, Otherwise };
        }

        public override VariableType CodeGen()
        {
            LinkedListNode<BasicBlock> thenBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> otherwiseBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> afterBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);

            // cond
            Cond.CodeGen();
            Constructor.AddJCond(thenBB.Value, otherwiseBB.Value);

            // then
            Constructor.CurrentBasicBlock = thenBB;
            Then.CodeGen();
            if (Constructor.CurrentBasicBlock.Value.Instructions.Last?.Value.IsBranch != true)
            {
                Constructor.AddJmp(afterBB.Value);
            }

            // otherwise
            Constructor.CurrentBasicBlock = otherwiseBB;
            Otherwise?.CodeGen();
            if (Constructor.CurrentBasicBlock.Value.Instructions.Last?.Value.IsBranch != true)
            {
                Constructor.AddJmp(afterBB.Value);
            }

            // after
            Constructor.CurrentBasicBlock = afterBB;

            return null;
        }
    }
}
