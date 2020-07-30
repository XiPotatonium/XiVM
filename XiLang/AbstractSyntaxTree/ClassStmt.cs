using XiVM;

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

        public override AST[] Children()
        {
            return new AST[] { Functions, Variables };
        }

        public override string ASTLabel()
        {
            return $"(Class){Id}";
        }

        public override VariableType CodeGen()
        {
            throw new System.NotImplementedException();
        }
    }
}
