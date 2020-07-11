namespace XiLang.AbstractSyntaxTree
{
    public abstract class DeclOrDefStmt : Stmt
    {
        public TypeExpr Type { set; get; }
        public string Id { private set; get; }

        public DeclOrDefStmt(TypeExpr type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}
