using System;
using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;
using XiLang.Errors;
using XiLang.Symbol;
using XiVM;
using XiVM.Xir;

namespace XiLang.Pass
{
    /// <summary>
    /// 注意CodeGen的过程会破坏AST，放在ASTPass的最后一个
    /// </summary>
    internal class CodeGenPass : IASTPass
    {
        public static CodeGenPass Singleton { get; } = new CodeGenPass();

        public static ModuleConstructor Constructor => Program.ModuleConstructor;

        public static Stack<BasicBlock> Breakable { private set; get; } = new Stack<BasicBlock>();
        public static Stack<BasicBlock> Continuable { private set; get; } = new Stack<BasicBlock>();
        /// <summary>
        /// 函数局部变量的符号栈
        /// </summary>
        public static SymbolTable LocalSymbolTable { set; get; }

        private CodeGenPass() { }

        public object Run(AST root)
        {
            // Import
            while (root != null && root is ImportStmt)
            {
                root.CodeGen();
                root = root.SiblingAST;
            }


            // 声明缓存，免得再找一遍
            List<ClassType> classes = new List<ClassType>();
            List<ClassField> fields = new List<ClassField>();
            List<Method> methods = new List<Method>();

            // 第一轮生成类的声明
            AST cur = root;
            ClassStmt classStmt;
            ClassType classType;
            while (cur != null)
            {
                classStmt = (ClassStmt)cur;
                classType = Constructor.AddClassType(classStmt.Id);
                classes.Add(classType);
                cur = cur.SiblingAST;
            }

            // 第二轮生成类方法和域的声明
            VarStmt varStmt;
            FuncStmt funcStmt;
            cur = root;
            List<ClassType>.Enumerator classesEnumerator = classes.GetEnumerator();
            while (cur != null)
            {
                classStmt = (ClassStmt)cur;
                classesEnumerator.MoveNext();
                classType = classesEnumerator.Current;

                varStmt = classStmt.Fields;
                while (varStmt != null)
                {
                    fields.Add(Constructor.AddClassField(classType, varStmt.Id, varStmt.Type.ToXirType(), varStmt.AccessFlag));
                    varStmt = (VarStmt)varStmt.SiblingAST;
                }

                funcStmt = classStmt.Methods;
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

                cur = cur.SiblingAST;
            }

            // 最后一轮生成类方法和域的定义
            classesEnumerator = classes.GetEnumerator();
            List<ClassField>.Enumerator fieldEnumerator = fields.GetEnumerator();
            List<Method>.Enumerator methodEnumerator = methods.GetEnumerator();
            cur = root;
            while (cur != null)
            {
                classStmt = (ClassStmt)cur;
                classesEnumerator.MoveNext();
                classType = classesEnumerator.Current;

                // 正在生成静态构造函数
                Constructor.CurrentBasicBlock = classType.StaticInitializer.BasicBlocks.First.Value;

                varStmt = classStmt.Fields;
                while (varStmt != null)
                {
                    fieldEnumerator.MoveNext();
                    ClassField field = fieldEnumerator.Current;

                    if (varStmt.AccessFlag.IsStatic == true)
                    {
                        if (varStmt.Init != null)
                        {
                            Constructor.AddGetStaticFieldAddress(field);
                            Constructor.AddLoadT(varStmt.Init.CodeGen());
                        }
                        // XiVM类变量默认全0的
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    varStmt = (VarStmt)varStmt.SiblingAST;
                }

                // 静态构造是return void，需要检查
                if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsRet != true)
                {
                    Constructor.AddRet();
                }

                funcStmt = classStmt.Methods;
                while (funcStmt != null)
                {
                    methodEnumerator.MoveNext();
                    Method method = methodEnumerator.Current;

                    // 函数局部变量栈
                    LocalSymbolTable = new SymbolTable();
                    LocalSymbolTable.PushFrame();

                    // 将参数加入符号表
                    VarStmt param = funcStmt.Params.Params;
                    foreach (Variable p in method.Params)
                    {
                        LocalSymbolTable.AddSymbol(param.Id, p);
                        param = (VarStmt)param.SiblingAST;
                    }

                    Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(method);
                    // 不要直接Body.CodeGen()，因为那样会新建一个NS
                    AST.CodeGen(funcStmt.Body.Child);

                    // 要检查XirGenPass.ModuleConstructor.CurrentBasicBlock最后一条Instruction是不是ret
                    if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsRet != true)
                    {
                        // 如果最后一条不是return
                        if (method.Type.ReturnType == null)
                        {
                            // 如果函数返回void，自动补上ret
                            Constructor.AddRet();
                        }
                        else
                        {
                            // 说明理论上应该返回值但是代码中没有return，报错
                            throw new XiLangError($"Function {param.Id} should return a value.");
                        }
                    }

                    // 清空函数局部变量栈
                    LocalSymbolTable = null;

                    funcStmt = (FuncStmt)funcStmt.SiblingAST;
                    Constructor.CompleteMethodGeneration(method);
                }

                cur = cur.SiblingAST;
            }

            return null;
        }
    }
}
