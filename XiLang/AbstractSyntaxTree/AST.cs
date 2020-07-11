namespace XiLang.AbstractSyntaxTree
{
    public abstract class AST
    {
        private static string PrintChildren(params AST[] children)
        {
            string ret = string.Empty;
            bool hasChild = false;
            foreach (AST child in children)
            {
                if (child == null)
                {
                    continue;
                }
                if (hasChild)
                {
                    ret += ", ";
                }
                else
                {
                    ret += ",\n\"children\": [";
                }
                ret += ToJson(child);
                hasChild = true;
            }
            if (hasChild)
            {
                ret += "]\n";
            }
            return ret;
        }

        public static string ToJson(AST ast)
        {
            string ret = $"{{\"name\": \"{ast.JsonName()}\" {PrintChildren(ast.JsonChildren())}}}";
            if (ast.SiblingAST != null)
            {
                ret += ", " + ToJson(ast.SiblingAST);
            }
            return ret;
        }

        public AST SiblingAST;

        /// <summary>
        /// 显示在Json文件中的名字
        /// </summary>
        /// <returns></returns>
        protected abstract string JsonName();
        /// <summary>
        /// 在Json中需要显示的孩子
        /// </summary>
        /// <returns></returns>
        protected abstract AST[] JsonChildren();
    }
}
