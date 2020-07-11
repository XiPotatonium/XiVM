namespace XiLang.AbstractSyntaxTree
{
    public class FuncStmt : DeclOrDefStmt
    {
        public BlockStmt Body { set; get; }
        public ParamsAst Params { private set; get; }

        public FuncStmt(TypeExpr type, string id, ParamsAst ps) : base(type, id)
        {
            Params = ps;
        }

        protected override string JsonName()
        {
            if (Body == null)
            {   // 函数声明
                return $"(FuncDecl){Id}";
            }
            return $"(FuncDef){Id}";
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Type, Params, Body };
        }
    }

    public class ParamsAst : AST
    {
        public VarStmt Params { set; get; }

        public ParamsAst(VarStmt ps)
        {
            Params = ps;
        }

        protected override string JsonName()
        {
            return "(Params)";
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Params };
        }
    }
}
