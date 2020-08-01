using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class VarStmt : DeclarationStmt
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

        public override VariableType CodeGen()
        {
            Variable var = Constructor.AddLocalVariable(Id, Type.ToXirType());

            // 初始化代码
            if (Init != null)
            {
                Init.CodeGen();                         // value
                Constructor.AddLocalA(var.StackOffset);      // addr
                Constructor.AddStoreT(var.Type);        // store
            }

            return null;
        }
    }
}
