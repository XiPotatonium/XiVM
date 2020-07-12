using XiLang.AbstractSyntaxTree;

namespace XiLang.PassMgr
{
    public interface IASTPass
    {
        object Run(AST root);
    }
}
