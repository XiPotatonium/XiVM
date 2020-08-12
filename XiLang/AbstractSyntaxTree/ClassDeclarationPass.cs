using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    internal class ClassDeclarationPass : IASTPass
    {
        public ModuleConstructor Constructor { get; }

        public ClassDeclarationPass(ModuleConstructor constructor)
        {
            Constructor = constructor;
        }

        public object Run(AST root)
        {
            // Import
            while (root != null && root is ImportStmt importStmt)
            {
                Program.Import(importStmt.Module.Id);

                root = root.SiblingAST;
            }

            // 声明缓存，免得再找一遍
            List<Class> classes = new List<Class>();

            // 第一轮生成类的声明
            ClassStmt classStmt;
            Class classType;
            while (root != null)
            {
                classStmt = (ClassStmt)root;
                classType = Constructor.AddClass(classStmt.Id);
                classes.Add(classType);
                root = root.SiblingAST;
            }

            return classes;
        }
    }
}
