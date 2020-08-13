using ConsoleArgumentParser;
using System;
using System.Collections.Generic;
using System.IO;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Lexical;
using XiLang.Syntactic;
using XiVM;
using XiVM.ConstantTable;
using XiVM.Xir;

namespace XiLang
{
    public class Program
    {
        public static readonly ClassType StringClass = new ClassType(SystemLib.Program.ModuleName, SystemLib.System.String.String.ClassName);
        public static readonly ObjectType StringObject = new ObjectType(StringClass);
        public static readonly ArrayType StringArray = new ArrayType(StringObject);

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
            List<Class> classes = (List<Class>)astPasses.Run(new ClassDeclarationPass(constructor));
            (List<Field> fields, List<Method> methods) = ((List<Field> fields, List<Method> methods))
                astPasses.Run(new MemberDeclarationPass(constructor, classes));
            astPasses.Run(new CodeGenPass(constructor, classes, fields, methods));

            // 输出生成字节码
            constructor.Dump(DirName, argumentParser.GetValue("verbose").IsSet);
        }

        public static void Import(string moduleName)
        {
            if (!ModuleHeaders.ContainsKey(moduleName))
            {
                // 避免重复导入
                if (File.Exists(Path.Combine(DirName, moduleName + ".xibc")))
                {
                    ModuleHeaders.Add(moduleName, BinaryModule.Load(Path.Combine(DirName, moduleName + ".xibc")));
                }
                else if (File.Exists(Path.Combine(DirName, moduleName + ".xi")))
                {
                    // 可能需要共同编译，因为也许互相有依赖
                    // 一个可能的想法是层序遍历，先声明所有的class，再声明所有的类成员，再定义所有的类成员
                    throw new NotImplementedException();
                }
                else
                {
                    throw new XiLangError($"{moduleName} not found");
                }
            }
        }


        public static List<(string descriptor, uint flag)> GetMethod(MemberType methodType)
        {
            if (!ModuleHeaders.TryGetValue(methodType.ClassType.ModuleName, out ModuleHeader header))
            {
                throw new XiLangError($"Module {methodType.ClassType.ModuleName} not imported but used");
            }

            List<(string descriptor, uint flag)> methodDescriptors = new List<(string descriptor, uint flag)>();
            foreach (MethodConstantInfo candidate in header.MethodPoolList)
            {
                if (header.StringPoolList[header.ClassPoolList[candidate.Class - 1].Name - 1] == methodType.ClassType.ClassName &&
                    header.StringPoolList[candidate.Name - 1] == methodType.Name)
                {
                    methodDescriptors.Add((header.StringPoolList[candidate.Descriptor - 1], candidate.Flag));
                }
            }
            return methodDescriptors;
        }

        public static int AssertClassExistence(string moduleName, string className)
        {
            ModuleHeaders.TryGetValue(moduleName, out ModuleHeader header);
            for (int i = 0; i < header.ClassPoolList.Count; ++i)
            {
                if (header.StringPoolList[header.ClassPoolList[i].Name - 1] == className)
                {
                    return i + 1;
                }
            }
            throw new XiLangError($"{moduleName}.{className} not found");
        }

        public static bool CheckFieldExistence(ModuleConstructor constructor, ClassType classType, string fieldName, out int index)
        {
            ModuleHeaders.TryGetValue(classType.ModuleName, out ModuleHeader header);

            int classInfoIndex = 1;
            foreach (ClassConstantInfo classInfo in header.ClassPoolList)
            {
                if (header.StringPoolList[classInfo.Name - 1] == classType.ClassName)
                {
                    break;
                }
                ++classInfoIndex;
            }

            foreach (FieldConstantInfo fieldInfo in header.FieldPoolList)
            {
                if (fieldInfo.Class == classInfoIndex && header.StringPoolList[fieldInfo.Name - 1] == fieldName)
                {
                    index = constructor.AddFieldPoolInfo(classType, fieldName,
                        header.StringPoolList[fieldInfo.Descriptor - 1], fieldInfo.Flag);
                    return true;
                }
            }
            index = -1;
            return false;
        }
    }
}
