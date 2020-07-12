namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        public AST SiblingAST;

        public abstract string ASTLabel();
        public abstract AST[] Children();
    }
}
