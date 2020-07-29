using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        /// <summary>
        /// 会同时CodeGen兄弟节点
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        public static XirValue CodeGen(AST ast)
        {
            XirValue ret = null;
            while (ast != null)
            {
                ret = ast.CodeGen();
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
        /// <returns></returns>
        public abstract XirValue CodeGen();
    }
}
