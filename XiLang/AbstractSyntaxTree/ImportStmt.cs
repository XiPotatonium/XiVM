using System;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal class ImportStmt : Stmt
    {
        public IdExpr Module { private set; get; }

        public ImportStmt(IdExpr module)
        {
            Module = module;
        }

        public override string ASTLabel()
        {
            return "import";
        }

        public override AST[] Children()
        {
            return new AST[] { Module };
        }

        public override VariableType CodeGen()
        {
            throw new NotImplementedException();
        }
    }
}
