using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Pass;
using XiLang.Syntactic;
using XiVM;
using XiVM.Xir;

namespace XiLang
{
    public class Program
    {
        public static List<BinaryModule> ImportedModules { private set; get; } = new List<BinaryModule>();
        public static ModuleConstructor ModuleConstructor { private set; get; }
        public static ClassType StringType { private set; get; }
        public static string DirName { private set; get; } = ".";

        public static void Main(string[] args)
        {
            ArgumentParser argumentParser = new ArgumentParser(new ConsoleArgument());
            argumentParser.AddArgument(new ConsoleArgument("d", ArgumentValueType.STRING));
            argumentParser.AddArgument(new ConsoleArgument("verbose"));

            argumentParser.Parse(args);

            string moduleName = argumentParser.GetValue().StringValue;
            ConsoleArgument dirArg = argumentParser.GetValue("d");
            if (dirArg.IsSet)
            {
                DirName = dirArg.StringValue;
            }

            string fileName = null;
            foreach (string f in Directory.EnumerateFiles(DirName))
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
                Console.Error.WriteLine($"Module {moduleName} not found in {DirName}");
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

            // 3，打印json文件
            if (argumentParser.GetValue("verbose").IsSet)
            {
                string json = (string)astPasses.Run(new JsonPass());
                File.WriteAllText(fileName + ".ast.json", json);
            }

            // 4，编译生成ir与字节码，编译阶段会完成常量表达式的估值
            ModuleConstructor = new ModuleConstructor(moduleName);
            astPasses.Run(CodeGenPass.Singleton);

            // 输出生成字节码
            ModuleConstructor.Dump(DirName, argumentParser.GetValue("verbose").IsSet);
        }

        public void Import(List<string> moduleName)
        {
            if (moduleName.Count == 0)
            {
                if (File.Exists(Path.Combine(DirName, moduleName[0], ".xir")))
                {

                }
                else if (File.Exists(Path.Combine(DirName, moduleName[0], ".xi")))
                {
                    // 可能需要共同编译，因为也许互相有依赖
                    throw new NotImplementedException();
                }
                else
                {
                    throw new XiLangError($"{moduleName[0]} not found");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 查找方法
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="className">类名</param>
        /// <param name="name">函数名</param>
        /// <param name="methodTypes">返回潜在函数列表</param>
        /// <returns></returns>
        public static bool TryGetMethod(string moduleName, string className, string name, out List<MethodType> methodTypes)
        {
            methodTypes = null;
            if (moduleName == ModuleConstructor.Module.Name)
            {
                foreach (ClassType classType in ModuleConstructor.Classes)
                {
                    if (classType.Name == className)
                    {
                        if (classType.Methods.TryGetValue(name, out List<Method> methods))
                        {
                            methodTypes = methods.Select(m => m.Type).ToList();
                            return true;
                        }
                        break;
                    }
                }
                return false;
            }
            throw new NotImplementedException();
        }
    }
}
