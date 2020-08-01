using XiLang.AbstractSyntaxTree;

namespace XiLang.Pass
{
    internal class JsonPass : IASTPass
    {
        public object Run(AST root)
        {
            // 递归打印
            return ToJson(root);
        }

        public string ToJson(AST ast)
        {
            string ret = $"{{\"name\": \"{ast.ASTLabel()}\" {PrintChildren(ast.Children())}}}";
            if (ast.SiblingAST != null)
            {
                ret += ", " + ToJson(ast.SiblingAST);
            }
            return ret;
        }

        private string PrintChildren(params AST[] children)
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
    }
}
