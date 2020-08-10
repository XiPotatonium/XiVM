using XiVM;

namespace XiLang.AbstractSyntaxTree
{
    internal abstract class AST
    {
        /// <summary>
        /// 会同时CodeGen兄弟节点
        /// </summary>
        /// <param name="ast"></param>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public static VariableType CodeGen(CodeGenPass pass, AST ast)
        {
            VariableType ret = null;
            while (ast != null)
            {
                ret = ast.CodeGen(pass);
                if (pass.Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsBranch == true)
                {
                    // Continue, break, return 这样的语句后面的兄弟没有必要再生成了
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
        /// <param name="pass"></param>
        /// <returns>如果是Expr返回Expr结果的Type，这个结果会保存在计算栈的栈顶；如果是Stmt，返回null</returns>
        public abstract VariableType CodeGen(CodeGenPass pass);
    }
}
