using XiLang.Errors;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        protected static ModuleConstructor Constructor => Program.ModuleConstructor;

        /// <summary>
        /// 会同时CodeGen兄弟节点
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public static VariableType CodeGen(AST ast)
        {
            VariableType ret = null;
            while (ast != null)
            {
                ret = ast.CodeGen();
                if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch == true)
                {
                    break;
                }
                ast = ast.SiblingAST;
            }
            return ret;
        }

        public AST SiblingAST;

        public abstract string ASTLabel();
        public abstract AST[] Children();

        /// <summary>
        /// 不会CodeGen兄弟
        /// </summary>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public abstract VariableType CodeGen();
    }
}
