using System;
using System.Collections.Generic;
using System.Text;
using XiLang.PassMgr;

namespace XiLang.AbstractSyntaxTree
{
    public class JsonPass : IASTPass
    {
        public object Run(AST root)
        {
            // 递归打印
            return ToJson(root);
        }

        public string ToJson(AST ast)
        {
            string ret = $"{{\"name\": \"{ast.JsonName()}\" {PrintChildren(ast.JsonChildren())}}}";
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
