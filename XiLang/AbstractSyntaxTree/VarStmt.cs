namespace XiLang.AbstractSyntaxTree
{
    public class VarStmt : DeclOrDefStmt
    {
        public Expr Init { private set; get; }

        public VarStmt(TypeExpr type, string id, Expr init) : base(type, id)
        {
            Init = init;
        }

        public override string JsonName()
        {
            if (Init != null)
            {
                return $"(VarDef){Id}";
            }
            return $"(VarDecl){Id}";
        }

        public override AST[] JsonChildren()
        {
            return new AST[] { Type, Init };
        }
    }
}
