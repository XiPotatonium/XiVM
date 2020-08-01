using XiLang.AbstractSyntaxTree;

namespace XiLang.Pass
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
