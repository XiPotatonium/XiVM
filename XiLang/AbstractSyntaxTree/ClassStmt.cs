using System;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class ClassStmt : Stmt
    {
        public string Id { set; get; }
        public FuncStmt Methods { set; get; }
        public VarStmt Fields { set; get; }

        public ClassStmt(string id)
        {
            Id = id;
        }

        public override AST[] Children()
        {
            return new AST[] { Methods, Fields };
        }

        public override string ASTLabel()
        {
            return $"(Class){Id}";
        }

        public override VariableType CodeGen()
        {
            throw new NotImplementedException();
        }
    }
}
