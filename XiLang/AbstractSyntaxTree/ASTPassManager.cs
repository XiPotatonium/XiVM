namespace XiLang.AbstractSyntaxTree
{
    internal interface IASTPass
    {
        object Run(AST root);
    }

    internal class ASTPassManager
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
