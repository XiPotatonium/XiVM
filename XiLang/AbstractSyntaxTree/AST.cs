namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        public AST SiblingAST;

        /// <summary>
        /// 显示在Json文件中的名字
        /// </summary>
        /// <returns></returns>
        public abstract string JsonName();
        /// <summary>
        /// 在Json中需要显示的孩子
        /// </summary>
        /// <returns></returns>
        public abstract AST[] JsonChildren();
    }
}
