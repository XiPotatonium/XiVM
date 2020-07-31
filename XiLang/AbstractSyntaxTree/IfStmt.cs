using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
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
            BasicBlock thenBB = CodeGenPass.Constructor.AddBasicBlock(CodeGenPass.Constructor.CurrentFunction);
            BasicBlock otherwiseBB = CodeGenPass.Constructor.AddBasicBlock(CodeGenPass.Constructor.CurrentFunction);
            BasicBlock afterBB = CodeGenPass.Constructor.AddBasicBlock(CodeGenPass.Constructor.CurrentFunction);

            // cond
            Cond.CodeGen();
            CodeGenPass.Constructor.AddJCond(thenBB, otherwiseBB);

            // then
            CodeGenPass.Constructor.CurrentBasicBlock = thenBB;
            Then.CodeGen();
            if (thenBB.Instructions.Last?.Value.IsBranch != true)
            {
                CodeGenPass.Constructor.AddJmp(afterBB);
            }

            // otherwise
            CodeGenPass.Constructor.CurrentBasicBlock = otherwiseBB;
            Otherwise?.CodeGen();
            if (otherwiseBB.Instructions.Last?.Value.IsBranch != true)
            {
                CodeGenPass.Constructor.AddJmp(afterBB);
            }

            // after
            CodeGenPass.Constructor.CurrentBasicBlock = afterBB;

            return null;
        }
    }
}
