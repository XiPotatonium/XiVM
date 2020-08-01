using System;
using System.Collections.Generic;
using XiLang.Pass;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public enum LoopType
    {
        WHILE, FOR
    }

    public class LoopStmt : Stmt
    {
        public static LoopStmt MakeFor(Stmt init, Expr cond, Expr step, BlockStmt body)
        {
            return new LoopStmt()
            {
                Type = LoopType.FOR,
                Init = init,
                Cond = cond,
                Step = step,
                Body = body
            };
        }

        public static LoopStmt MakeWhile(Expr cond, BlockStmt body)
        {
            return new LoopStmt()
            {
                Type = LoopType.WHILE,
                Cond = cond,
                Body = body
            };
        }

        public LoopType Type { private set; get; }
        /// <summary>
        /// 只能是VarStmt或者ExprStmt
        /// </summary>
        public Stmt Init { private set; get; }
        public Expr Cond { private set; get; }
        public Expr Step { private set; get; }
        public BlockStmt Body { private set; get; }


        public override string ASTLabel()
        {
            return Type switch
            {
                LoopType.WHILE => "while",
                LoopType.FOR => "for",
                _ => throw new NotImplementedException(),
            };
        }

        public override AST[] Children()
        {
            return new AST[] { Init, Cond, Step, Body };
        }

        public override VariableType CodeGen()
        {
            switch (Type)
            {
                case LoopType.WHILE:
                    WhileCodeGen();
                    break;
                case LoopType.FOR:
                    ForCodeGen();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return null;
        }

        private void ForCodeGen()
        {
            Constructor.SymbolTable.Push();
            LinkedListNode<BasicBlock> condBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> bodyBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> stepBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> afterBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);

            CodeGenPass.Breakable.Push(afterBB);
            CodeGenPass.Continuable.Push(stepBB);

            // pre head
            Init?.CodeGen();
            Constructor.AddJmp(condBB.Value);

            // cond
            Constructor.CurrentBasicBlock = condBB;
            if (Cond == null)
            {
                Constructor.AddJmp(bodyBB.Value);
            }
            else
            {
                Cond.CodeGen();
                if (Constructor.CurrentBasicBlock.Value.Instructions.Last?.Value.IsBranch != true)
                {
                    Constructor.AddJCond(bodyBB.Value, afterBB.Value);
                }
            }

            // body
            Constructor.CurrentBasicBlock = bodyBB;
            if (Body != null)
            {
                // 不要直接CodeGen Body，因为那样会新建一个NS
                CodeGen(Body.Child);
            }
            Constructor.AddJmp(stepBB.Value);

            // step
            Constructor.CurrentBasicBlock = stepBB;
            CodeGen(Step);      // Step可能有好几个expr形成一个list
            Constructor.AddJmp(condBB.Value);

            // after
            Constructor.CurrentBasicBlock = afterBB;
            CodeGenPass.Breakable.Pop();
            CodeGenPass.Continuable.Pop();
        }

        private void WhileCodeGen()
        {
            LinkedListNode<BasicBlock> condBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> bodyBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);
            LinkedListNode<BasicBlock> afterBB = Constructor.AddBasicBlock(Constructor.CurrentFunction);

            CodeGenPass.Breakable.Push(afterBB);
            CodeGenPass.Continuable.Push(condBB);

            // pre head
            Constructor.AddJmp(condBB.Value);

            // cond
            Constructor.CurrentBasicBlock = condBB;
            Cond.CodeGen();
            Constructor.AddJCond(bodyBB.Value, afterBB.Value);

            // body
            Constructor.CurrentBasicBlock = bodyBB;
            Body?.CodeGen();
            if (Constructor.CurrentBasicBlock.Value.Instructions.Last?.Value.IsBranch != true)
            {
                Constructor.AddJmp(afterBB.Value);
            }

            // after
            Constructor.CurrentBasicBlock = afterBB;
            CodeGenPass.Breakable.Pop();
            CodeGenPass.Continuable.Pop();
        }
    }
}
