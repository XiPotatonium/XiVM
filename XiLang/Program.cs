using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.IO;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Lexical;
using XiLang.Syntactic;
using XiVM;
using XiVM.Xir;

namespace XiLang
{
    public class Program
    {
        /// <summary>
        /// TODO
        /// </summary>
        public static ClassType StringType { private set; get; }

        public static Dictionary<string, ModuleHeader> ModuleHeaders { private set; get; } = new Dictionary<string, ModuleHeader>();
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

            // 解析类信息之外的部分并生成AST
            AST root = (AST)tokenPasses.Run(new Parser());

            Console.WriteLine("Parse done!");


            ASTPassManager astPasses = new ASTPassManager(root);

            //, 打印json文件
            if (argumentParser.GetValue("verbose").IsSet)
            {
                string json = (string)astPasses.Run(new JsonPass());
                File.WriteAllText(fileName + ".ast.json", json);
            }

            // 编译生成ir与字节码，编译阶段会完成常量表达式的估值
            ModuleConstructor constructor = new ModuleConstructor(moduleName);
            ModuleHeaders.Add(moduleName, constructor.Module);
            List<ClassType> classes = (List<ClassType>)astPasses.Run(new ClassDeclarationPass(constructor));
            (List<ClassField> fields, List<Method> methods) = ((List<ClassField> fields, List<Method> methods))
                astPasses.Run(new MemberDeclarationPass(constructor, classes));
            astPasses.Run(new CodeGenPass(constructor, classes, fields, methods));

            // 输出生成字节码
            constructor.Dump(DirName, argumentParser.GetValue("verbose").IsSet);
        }

        public static void Import(List<string> moduleName)
        {
            if (moduleName.Count == 1)
            {
                if (!ModuleHeaders.ContainsKey(moduleName[0]))
                {
                    // 避免重复导入
                    if (File.Exists(Path.Combine(DirName, moduleName[0] + ".xibc")))
                    {
                        ModuleHeaders.Add(moduleName[0], BinaryModule.Load(Path.Combine(DirName, moduleName[0] + ".xibc")));
                    }
                    else if (File.Exists(Path.Combine(DirName, moduleName[0] + ".xi")))
                    {
                        // 可能需要共同编译，因为也许互相有依赖
                        // 一个可能的想法是层序遍历，先声明所有的class，再声明所有的类成员，再定义所有的类成员
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new XiLangError($"{moduleName[0]} not found");
                    }
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
        /// <returns>返回潜在函数列表</returns>
        public static List<(string descriptor, uint flag)> GetMethod(string moduleName, string className, string name)
        {
            if (!ModuleHeaders.TryGetValue(moduleName, out ModuleHeader header))
            {
                throw new XiLangError($"Module {moduleName} not imported but used");
            }
            else
            {
                List<(string descriptor, uint flag)> methodDescriptors = new List<(string descriptor, uint flag)>();
                foreach (XiVM.ConstantTable.MethodConstantInfo candidate in header.MethodPoolList)
                {
                    if (header.StringPoolList[header.ClassPoolList[candidate.Class - 1].Name - 1] == className &&
                        header.StringPoolList[candidate.Name - 1] == name)
                    {
                        methodDescriptors.Add((header.StringPoolList[candidate.Type - 1], candidate.Flag));
                    }
                }
                return methodDescriptors;
            }
        }
    }
}
