namespace XiLang.AbstractSyntaxTree
{
    public class VarStmt : DeclOrDefStmt
    {
        public Expr Init { private set; get; }

        public VarStmt(TypeExpr type, string id, Expr init) : base(type, id)
        {
            Init = init;
        }

        public override string ASTLabel()
        {
            if (Init != null)
            {
                return $"(VarDef){Id}";
            }
            return $"(VarDecl){Id}";
        }

        public override AST[] Children()
        {
            return new AST[] { Type, Init };
        }
    }
}
