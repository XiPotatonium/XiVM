using XiLang.AbstractSyntaxTree;

namespace XiLang.PassMgr
{
    public class ASTPassManager
    {
        public AST Root { get; }

        public ASTPassManager(AST root)
        {
            Root = root;
        }

        public object Run(IASTPass pass)
        {
            return pass.Run(Root);
        }
    }
}
