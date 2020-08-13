using System.Text;

namespace XiLang.AbstractSyntaxTree
{
    internal class JsonPass : IASTPass
    {
        public StringBuilder StringBuilder { private set; get; }

        public object Run(AST root)
        {
            StringBuilder = new StringBuilder("{\n");
            // 递归打印
            ToJson(root);
            StringBuilder.Append("\n}");

            return StringBuilder.ToString();
        }

        public void ToJson(AST ast)
        {
            StringBuilder.Append("{\"name\": \"").Append(ast.ASTLabel()).Append("\" ");
            PrintChildren(ast.Children());
            StringBuilder.Append('}');
            if (ast.SiblingAST != null)
            {
                StringBuilder.Append(", ");
                ToJson(ast.SiblingAST);
            }
        }

        private void PrintChildren(params AST[] children)
        {
            bool hasChild = false;
            foreach (AST child in children)
            {
                if (child == null)
                {
                    continue;
                }
                if (hasChild)
                {
                    StringBuilder.Append(", ");
                }
                else
                {
                    StringBuilder.Append(",\n\"children\": [");
                }
                ToJson(child);
                hasChild = true;
            }
            if (hasChild)
            {
                StringBuilder.Append("]\n");
            }
        }
    }
}
