using System;
using System.Collections.Generic;
using System.IO;
using XiLang.AbstractSyntaxTree;
using XiLang.PassMgr;
using XiLang.Syntactic;

namespace XiLang
{
    internal class Program
    {
        private static readonly string ExprTest0 = "int[] foo(int a, float[] b) {      // 这样的注释\n" +
            "   int c = b[12 * (a + a | (int)b[0] << 3)].bar(a > 10 ? 2.0 : b[4]); \n" +
            "   return b;}\n" +
            "// 这tm是一条注释";

        private static readonly string BasicTest0 = "int foo(int a, float[] b) {      // 这样的注释\n" +
            "   int c = a + b[2]; \n" +
            "   return c;}\n" +
            "// 这tm是一条注释";

        private static readonly string ClassTest0 = "int a = 10;\n" +
            "class Demo {\n" +
            "   int id = 0;\n" +
            "   float val = 10.0;\n" +
            "   int getId() { return id; }\n" +
            "}\n" +
            "int main(int argc, string[] argv) { Demo d; return 0; }";

        private static readonly string ConstExprTest0 = "int main(int argc, string[] argv) {" +
            "   int a = 2;\n" +
            "   if (80 > 0.0) { return a * (4 + 16 % 3); }\n" +
            "   return 0; }";

        private static void Main(string[] args)
        {
            TokenPassManager tokenPasses = new TokenPassManager(ConstExprTest0);

            // 第一个pass，获取所有类信息，因此我们的语法不允许分离类定义和声明
            HashSet<string> classes = (HashSet<string>)tokenPasses.Run(new ClassPass());

            // 第二个pass，解析类信息之外的部分并生成AST
            AST root = (AST)tokenPasses.Run(new Parser(classes));

            Console.WriteLine("Parse done!");

            ASTPassManager astPasses = new ASTPassManager(root);

            // 第三个pass，常量表达式直接估值
            astPasses.Run(new ConstExprPass());

            // 第四个pass，打印json文件，仅调试需要
            string json = (string)astPasses.Run(new JsonPass());
            File.WriteAllText("ast.json", json);
        }
    }
}
