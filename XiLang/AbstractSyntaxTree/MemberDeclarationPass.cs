using System.Collections.Generic;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    internal class MemberDeclarationPass : IASTPass
    {
        public List<ClassType> Classes { get; }
        public ModuleConstructor Constructor { get; }

        public MemberDeclarationPass(ModuleConstructor constructor, List<ClassType> classes)
        {
            Classes = classes;
            Constructor = constructor;
        }

        public object Run(AST root)
        {
            // 跳过Import
            while (root != null && root is ImportStmt)
            {
                root = root.SiblingAST;
            }

            List<ClassField> fields = new List<ClassField>();
            List<Method> methods = new List<Method>();

            // 第二轮生成类方法和域的声明
            List<ClassType>.Enumerator classesEnumerator = Classes.GetEnumerator();
            while (root != null)
            {
                ClassStmt classStmt = (ClassStmt)root;
                classesEnumerator.MoveNext();
                ClassType classType = classesEnumerator.Current;

                VarStmt varStmt = classStmt.Fields;
                while (varStmt != null)
                {
                    fields.Add(Constructor.AddClassField(classType, varStmt.Id, varStmt.Type.ToXirType(), varStmt.AccessFlag));
                    varStmt = (VarStmt)varStmt.SiblingAST;
                }

                FuncStmt funcStmt = classStmt.Methods;
                while (funcStmt != null)
                {
                    List<VariableType> pTypes = new List<VariableType>();
                    VarStmt param = funcStmt.Params.Params;
                    while (param != null)
                    {
                        pTypes.Add(param.Type.ToXirType());
                        param = (VarStmt)param.SiblingAST;
                    }
                    methods.Add(Constructor.AddMethod(classType, funcStmt.Id,
                        Constructor.AddMethodType(funcStmt.Type.ToXirType(), pTypes),
                        funcStmt.AccessFlag));

                    funcStmt = (FuncStmt)funcStmt.SiblingAST;
                }

                root = root.SiblingAST;
            }

            return (fields, methods);
        }
    }
}
