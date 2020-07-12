﻿namespace XiLang.AbstractSyntaxTree
{
    public class FuncStmt : DeclOrDefStmt
    {
        public BlockStmt Body { set; get; }
        public ParamsAst Params { private set; get; }

        public FuncStmt(TypeExpr type, string id, ParamsAst ps) : base(type, id)
        {
            Params = ps;
        }

        public override string ASTLabel()
        {
            if (Body == null)
            {   // 函数声明
                return $"(FuncDecl){Id}";
            }
            return $"(FuncDef){Id}";
        }

        public override AST[] Children()
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

        public override string ASTLabel()
        {
            return "(Params)";
        }

        public override AST[] Children()
        {
            return new AST[] { Params };
        }
    }
}
