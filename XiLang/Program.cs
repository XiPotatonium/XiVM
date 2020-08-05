using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.IO;
using XiLang.AbstractSyntaxTree;
using XiLang.Pass;
using XiLang.Syntactic;
using XiVM.Xir;

namespace XiLang
{
    public class Program
    {
        public static ModuleConstructor ModuleConstructor { private set; get; }

        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());
            argumentParser.AddArgument(new ConsoleArgument("d", ArgumentValueType.STRING));
            argumentParser.AddArgument(new ConsoleArgument("verbose"));

            argumentParser.Parse(args);

            string moduleName = argumentParser.GetValue().StringValue;
            ConsoleArgument dirArg = argumentParser.GetValue("d");
            string dirName = ".";
            if (dirArg.IsSet)
            {
                dirName = dirArg.StringValue;
            }

            string fileName = null;
            foreach (string f in Directory.EnumerateFiles(dirName))
            {
                string fname = Path.GetFileName(f).ToString();
                if (fname.StartsWith(moduleName + "."))
                {
                    fileName = f;
                    break;  // TODO 不break，支持同模块多文件
                }
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Console.Error.WriteLine($"Module {moduleName} not found in {dirName}");
                return;
            }

            string text = File.ReadAllText(fileName);

            TokenPassManager tokenPasses = new TokenPassManager(text);

            // 1，获取所有类信息，我们的语法不允许分离类定义和声明
            HashSet<string> classes = (HashSet<string>)tokenPasses.Run(new ClassPass());

            // 2，解析类信息之外的部分并生成AST
            AST root = (AST)tokenPasses.Run(new Parser(classes));

            Console.WriteLine("Parse done!");

            ASTPassManager astPasses = new ASTPassManager(root);

            // 3，常量表达式直接估值
            astPasses.Run(new ConstExprPass());

            // 4，打印json文件
            if (argumentParser.GetValue("verbose").IsSet)
            {
                string json = (string)astPasses.Run(new JsonPass());
                File.WriteAllText(fileName + ".ast.json", json);
            }

            // 5，编译生成ir与字节码
            ModuleConstructor = new ModuleConstructor(moduleName);
            astPasses.Run(CodeGenPass.Singleton);

            // 输出生成字节码
            ModuleConstructor.Dump(dirName, argumentParser.GetValue("verbose").IsSet);
        }
    }
}
