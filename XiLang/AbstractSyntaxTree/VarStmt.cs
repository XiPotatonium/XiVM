using System.Text;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal class VarStmt : DeclarationStmt
    {
        /// <summary>
        /// 变量定义的初始化表达式
        /// </summary>
        public Expr Init { private set; get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessFlag">局部变量传null</param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="init"></param>
        public VarStmt(AccessFlag accessFlag, TypeExpr type, string id, Expr init)
            : base(accessFlag, type, id)
        {
            Init = init;
        }

        public override string ASTLabel()
        {
            StringBuilder sb = new StringBuilder();
            if (AccessFlag.IsStatic == true)
            {
                sb.Append("static ");
            }

            sb.Append("(VarDecl)");
            sb.Append(Id);
            return sb.ToString();
        }

        public override AST[] Children()
        {
            return new AST[] { Type, Init };
        }

        /// <summary>
        /// 此处生成local变量
        /// </summary>
        /// <returns></returns>
        public override VariableType CodeGen(CodeGenPass pass)
        {
            Variable var = pass.Constructor.AddLocalVariable(Id, Type.ToXirType(pass.Constructor));
            pass.LocalSymbolTable.AddSymbol(Id, var);

            // 初始化代码
            if (Init != null)
            {
                Init.CodeGen(pass);                         // value
                pass.Constructor.AddLocal(var.Offset);      // addr
                pass.Constructor.AddStoreT(var.Type);       // store
                pass.Constructor.AddPop(var.Type);
            }

            return null;
        }
    }
}
