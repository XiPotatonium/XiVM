using System.Collections.Generic;
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
            BasicBlock thenBB = Constructor.AddBasicBlock(Constructor.CurrentMethod);
            BasicBlock otherwiseBB = Constructor.AddBasicBlock(Constructor.CurrentMethod);
            BasicBlock afterBB = Constructor.AddBasicBlock(Constructor.CurrentMethod);

            // cond
            Cond.CodeGen();
            Constructor.AddJCond(thenBB, otherwiseBB);

            // then
            Constructor.CurrentBasicBlock = thenBB;
            Then.CodeGen();
            if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
            {
                Constructor.AddJmp(afterBB);
            }

            // otherwise
            Constructor.CurrentBasicBlock = otherwiseBB;
            Otherwise?.CodeGen();
            if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
            {
                Constructor.AddJmp(afterBB);
            }

            // after
            Constructor.CurrentBasicBlock = afterBB;

            return null;
        }
    }
}
