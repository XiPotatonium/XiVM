using System;
using System.Text;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal class FuncStmt : DeclarationStmt
    {
        public BlockStmt Body { set; get; }
        public ParamsAst Params { private set; get; }

        public FuncStmt(AccessFlag flag, TypeExpr type, string id, ParamsAst ps)
            : base(flag, type, id)
        {
            Params = ps;
        }

        public override string ASTLabel()
        {
            StringBuilder sb = new StringBuilder();
            if (AccessFlag.IsStatic == true)
            {
                sb.Append("static ");
            }

            sb.Append("(FuncDecl)");
            sb.Append(Id);
            return sb.ToString();
        }

        public override AST[] Children()
        {
            return new AST[] { Type, Params, Body };
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            throw new NotImplementedException();
        }
    }

    internal class ParamsAst : AST
    {
        public VarStmt Params { set; get; }

        public ParamsAst(VarStmt ps)
        {
            Params = ps;
        }

        public override string ASTLabel()
        {
            return "(Params)";
        }

        public override AST[] Children()
        {
            return new AST[] { Params };
        }

        public override VariableType CodeGen(CodeGenPass pass)
        {
            throw new NotImplementedException();
        }
    }
}
