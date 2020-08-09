using System;
using System.Text;
using XiLang.Pass;
using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    public class VarStmt : DeclarationStmt
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
        public override VariableType CodeGen()
        {
            Variable var = Constructor.AddLocalVariable(Id, Type.ToXirType());
            CodeGenPass.LocalSymbolTable.AddSymbol(Id, var);

            // 初始化代码
            if (Init != null)
            {
                Init.CodeGen();                         // value
                Constructor.AddLocal(var.Offset);       // addr
                Constructor.AddStoreT(var.Type);        // store
            }

            return null;
        }
    }
}
