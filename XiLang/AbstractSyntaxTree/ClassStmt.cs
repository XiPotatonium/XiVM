namespace XiLang.AbstractSyntaxTree
{
    public class ClassStmt : Stmt
    {
        public string Id { set; get; }
        public FuncStmt Functions { set; get; }
        public VarStmt Variables { set; get; }

        public ClassStmt(string id)
        {
            Id = id;
        }

        protected override AST[] JsonChildren()
        {
            return new AST[] { Functions, Variables };
        }

        protected override string JsonName()
        {
            return $"(Class){Id}";
        }
    }
}
