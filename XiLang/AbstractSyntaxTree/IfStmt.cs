using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    internal class IfStmt : Stmt
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

        public override VariableType CodeGen(CodeGenPass pass)
        {
            BasicBlock thenBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock otherwiseBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);
            BasicBlock afterBB = pass.Constructor.AddBasicBlock(pass.Constructor.CurrentMethod);

            // cond
            Cond.CodeGen(pass);
            pass.Constructor.AddJCond(thenBB, otherwiseBB);

            // then
            pass.Constructor.CurrentBasicBlock = thenBB;
            Then.CodeGen(pass);
            if (pass.Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
            {
                pass.Constructor.AddJmp(afterBB);
            }

            // otherwise
            pass.Constructor.CurrentBasicBlock = otherwiseBB;
            Otherwise?.CodeGen(pass);
            if (pass.Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch != true)
            {
                pass.Constructor.AddJmp(afterBB);
            }

            // after
            pass.Constructor.CurrentBasicBlock = afterBB;

            return null;
        }
    }
}
