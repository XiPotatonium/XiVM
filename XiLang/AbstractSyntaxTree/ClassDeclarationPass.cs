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
                List<string> moduleName = new List<string>();
                IdExpr curId = importStmt.Module;
                while (curId != null)
                {
                    moduleName.Add(curId.Id);
                    curId = (IdExpr)curId.SiblingAST;
                }
                Program.Import(moduleName);

                root = root.SiblingAST;
            }

            // 声明缓存，免得再找一遍
            List<ClassType> classes = new List<ClassType>();

            // 第一轮生成类的声明
            ClassStmt classStmt;
            ClassType classType;
            while (root != null)
            {
                classStmt = (ClassStmt)root;
                classType = Constructor.AddClassType(classStmt.Id);
                classes.Add(classType);
                root = root.SiblingAST;
            }

            return classes;
        }
    }
}
