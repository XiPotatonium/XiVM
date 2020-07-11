namespace XiLang.AbstractSyntaxTree
{
    public class VarStmt : DeclOrDefStmt
    {
        public Expr Init { private set; get; }

        public VarStmt(TypeExpr type, string id, Expr init) : base(type, id)
        {
            Init = init;
        }

        protected override string JsonName()
        {
            if (Init != null)
            {
                return $"(VarDef){Id}";
            }
            return $"(VarDecl){Id}";
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Type, Init };
        }
    }
}
