using System;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public enum LoopType
    {
        WHILE, FOR
    }

    internal class LoopStmt : Stmt
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

        public override VariableType CodeGen(CodeGenPass pass)
        {
            switch (Type)
            {
                case LoopType.WHILE:
                    WhileCodeGen(pass);
                    break;
                case LoopType.FOR:
                    ForCodeGen(pass);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return null;
        }

        private void ForCodeGen(CodeGenPass pass)
        {
            pass.LocalSymbolTable.PushFrame();
            BasicBlock condBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock bodyBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock stepBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock afterBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);

            pass.Breakable.Push(afterBB);
            pass.Continuable.Push(stepBB);

            // pre head
            Init?.CodeGen(pass);
            pass.Constructor.AddJmp(condBB);

            // cond
            pass.Constructor.CurrentBasicBlock = condBB;
            if (Cond == null)
            {
                pass.Constructor.AddJmp(bodyBB);
            }
            else
            {
                Cond.CodeGen(pass);
                if (pass.Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
                {
                    pass.Constructor.AddJCond(bodyBB, afterBB);
                }
            }

            // body
            pass.Constructor.CurrentBasicBlock = bodyBB;
            if (Body != null)
            {
                // 不要直接CodeGen Body，因为那样会新建一个NS
                CodeGen(pass, Body.Child);
            }
            pass.Constructor.AddJmp(stepBB);

            // step
            pass.Constructor.CurrentBasicBlock = stepBB;
            CodeGen(pass, Step);      // Step可能有好几个expr形成一个list
            pass.Constructor.AddJmp(condBB);

            // after
            pass.Constructor.CurrentBasicBlock = afterBB;
            pass.Breakable.Pop();
            pass.Continuable.Pop();
            pass.LocalSymbolTable.PopFrame();
        }

        private void WhileCodeGen(CodeGenPass pass)
        {
            BasicBlock condBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock bodyBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock afterBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);

            pass.Breakable.Push(afterBB);
            pass.Continuable.Push(condBB);

            // pre head
            pass.Constructor.AddJmp(condBB);

            // cond
            pass.Constructor.CurrentBasicBlock = condBB;
            Cond.CodeGen(pass);
            pass.Constructor.AddJCond(bodyBB, afterBB);

            // body
            pass.Constructor.CurrentBasicBlock = bodyBB;
            Body?.CodeGen(pass);
            if (pass.Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
            {
                pass.Constructor.AddJmp(afterBB);
            }

            // after
            pass.Constructor.CurrentBasicBlock = afterBB;
            pass.Breakable.Pop();
            pass.Continuable.Pop();
        }
    }
}
