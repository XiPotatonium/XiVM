using XiLang.AbstractSyntaxTree;

namespace XiLang.Pass
{
    public interface IASTPass
    {
        object Run(AST root);
    }
}
