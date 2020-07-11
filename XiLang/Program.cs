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
#pragma warning disable CS0414 // The field 'Program.ExprTest0' is assigned but its value is never used
        private static readonly string ExprTest0 = "i32[] foo(i32 a, f32[] b) {      // 这样的注释\n" +
#pragma warning restore CS0414 // The field 'Program.ExprTest0' is assigned but its value is never used
            "   i32 c = b[12 * (a + a | (i32)b[0] << 3)].bar(a > 10 ? 2.0 : b[4]); \n" +
            "   return b;}\n" +
            "// 这tm是一条注释";

#pragma warning disable CS0414 // The field 'Program.BasicTest0' is assigned but its value is never used
        private static readonly string BasicTest0 = "i32 foo(i32 a, f32[] b) {      // 这样的注释\n" +
#pragma warning restore CS0414 // The field 'Program.BasicTest0' is assigned but its value is never used
            "   i32 c = a + b[2]; \n" +
            "   return c;}\n" +
            "// 这tm是一条注释";

        private static readonly string ClassTest0 = "i32 a = 10;\n" +
            "class Demo {\n" +
            "   i32 id = 0;\n" +
            "   f32 val = 10.0;\n" +
            "   i32 getId() { return id; }\n" +
            "}\n" +
            "i32 main(i32 argc, string[] argv) { Demo d; return 0; }";

        private static void Main(string[] args)
        {
            PassManager passManager = new PassManager(ClassTest0);

            // 第一个Pass，获取所有类信息，因此我们的语法不允许分离类定义和声明
            HashSet<string> classes = (HashSet<string>)passManager.Run(new ClassPass());

            // 第二个Pass，解析类信息之外的部分并生成AST
            AST root = (AST)passManager.Run(new Parser(classes));

            Console.WriteLine("Parse done!");

            File.WriteAllText("ast.json", AST.ToJson(root));
        }
    }
}
