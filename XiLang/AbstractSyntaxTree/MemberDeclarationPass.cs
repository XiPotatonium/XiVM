using System.Collections.Generic;
using XiLang.Errors;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    internal class MemberDeclarationPass : IASTPass
    {
        public List<Class> Classes { get; }
        public ModuleConstructor Constructor { get; }

        public MemberDeclarationPass(ModuleConstructor constructor, List<Class> classes)
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

            List<Field> fields = new List<Field>();
            List<Method> methods = new List<Method>();

            // 第二轮生成类方法和域的声明
            List<Class>.Enumerator classesEnumerator = Classes.GetEnumerator();
            while (root != null)
            {
                ClassStmt classStmt = (ClassStmt)root;
                classesEnumerator.MoveNext();
                Class currentClass = classesEnumerator.Current;

                VarStmt varStmt = classStmt.Fields;
                while (varStmt != null)
                {
                    fields.Add(Constructor.AddClassField(currentClass, varStmt.Id, varStmt.Type.ToXirType(Constructor), varStmt.AccessFlag));
                    varStmt = (VarStmt)varStmt.SiblingAST;
                }

                FuncStmt funcStmt = classStmt.Methods;
                while (funcStmt != null)
                {
                    List<VariableType> pTypes = new List<VariableType>();
                    VarStmt param = funcStmt.Params.Params;
                    while (param != null)
                    {
                        pTypes.Add(param.Type.ToXirType(Constructor));
                        param = (VarStmt)param.SiblingAST;
                    }

                    VariableType retType = funcStmt.Type.ToXirType(Constructor);
                    if (funcStmt.Id == "(init)")
                    {
                        // AST上缺少了函数名，检查是否符合构造函数条件
                        if (retType is ObjectType objectType &&
                            objectType.ClassName == currentClass.Name && objectType.ModuleName == currentClass.Parent.Name)
                        {
                            // 确实是构造函数，返回值改为void
                            methods.Add(Constructor.AddMethod(currentClass, funcStmt.Id,
                                null, pTypes, funcStmt.AccessFlag));
                        }
                        else
                        {
                            throw new XiLangError("Missing return type or mis-spelled constructor");
                        }
                    }
                    else
                    {
                        methods.Add(Constructor.AddMethod(currentClass, funcStmt.Id,
                            retType, pTypes, funcStmt.AccessFlag));
                    }

                    funcStmt = (FuncStmt)funcStmt.SiblingAST;
                }

                root = root.SiblingAST;
            }

            return (fields, methods);
        }
    }
}
