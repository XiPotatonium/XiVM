using System;
using System.Collections.Generic;
using XiLang.Errors;
using XiLang.Symbol;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    /// <summary>
    /// 注意CodeGen的过程会破坏AST，放在ASTPass的最后一个
    /// </summary>
    internal class CodeGenPass : IASTPass
    {
        public Stack<BasicBlock> Breakable { get; }
        public Stack<BasicBlock> Continuable { get; }
        public ModuleConstructor Constructor { get; }

        /// <summary>
        /// 函数局部变量的符号栈
        /// </summary>
        public SymbolTable LocalSymbolTable { set; get; }


        private List<ClassField> Fields { get; }
        private List<Method> Methods { get; }
        private List<ClassType> Classes { get; }

        public CodeGenPass(ModuleConstructor constructor, List<ClassType> classes, List<ClassField> fields, List<Method> methods)
        {
            Constructor = constructor;
            Classes = classes;
            Fields = fields;
            Methods = methods;
            Breakable = new Stack<BasicBlock>();
            Continuable = new Stack<BasicBlock>();
            LocalSymbolTable = null;
        }

        public object Run(AST root)
        {
            // 跳过Import
            while (root != null && root is ImportStmt)
            {
                root = root.SiblingAST;
            }

            // 最后一轮生成类方法和域的定义
            List<ClassType>.Enumerator classesEnumerator = Classes.GetEnumerator();
            List<ClassField>.Enumerator fieldEnumerator = Fields.GetEnumerator();
            List<Method>.Enumerator methodEnumerator = Methods.GetEnumerator();
            while (root != null)
            {
                ClassStmt classStmt = (ClassStmt)root;
                classesEnumerator.MoveNext();
                ClassType classType = classesEnumerator.Current;

                // 正在生成静态构造函数
                Constructor.CurrentBasicBlock = classType.StaticInitializer.BasicBlocks.First.Value;

                VarStmt varStmt = classStmt.Fields;
                while (varStmt != null)
                {
                    fieldEnumerator.MoveNext();
                    ClassField field = fieldEnumerator.Current;

                    if (varStmt.AccessFlag.IsStatic == true)
                    {
                        if (varStmt.Init != null)
                        {
                            VariableType variableType = varStmt.Init.CodeGen(this);
                            Constructor.AddGetStaticFieldAddress(field);
                            Constructor.AddAStoreT(variableType);
                            Constructor.AddPop(variableType);
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

                FuncStmt funcStmt = classStmt.Methods;
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
                    AST.CodeGen(this, funcStmt.Body.Child);

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

                root = root.SiblingAST;
            }

            return null;
        }
    }
}
