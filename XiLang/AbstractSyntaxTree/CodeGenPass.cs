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
        /// <summary>
        /// 缓存，免得每次生成构造函数都要重新找一遍
        /// </summary>
        private Dictionary<Class, List<(VarStmt, ClassField)>> NonStaticFields { set; get; }
        private List<Method> Methods { get; }
        private List<Class> Classes { get; }

        public CodeGenPass(ModuleConstructor constructor, List<Class> classes, List<ClassField> fields, List<Method> methods)
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
            List<Class>.Enumerator classesEnumerator = Classes.GetEnumerator();
            List<ClassField>.Enumerator fieldEnumerator = Fields.GetEnumerator();
            List<Method>.Enumerator methodEnumerator = Methods.GetEnumerator();
            NonStaticFields = new Dictionary<Class, List<(VarStmt, ClassField)>>();
            while (root != null)
            {
                ClassStmt classStmt = (ClassStmt)root;
                classesEnumerator.MoveNext();
                Class classType = classesEnumerator.Current;

                // 正在生成静态构造函数
                Constructor.CurrentBasicBlock = classType.StaticInitializer.BasicBlocks.First.Value;

                VarStmt varStmt = classStmt.Fields;
                List<(VarStmt, ClassField)> nonStaticFields = new List<(VarStmt, ClassField)>();
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
                        nonStaticFields.Add((varStmt, field));
                        // 缓存，免得每次生成构造函数都再找一遍
                    }

                    varStmt = (VarStmt)varStmt.SiblingAST;
                }
                NonStaticFields.Add(classType, nonStaticFields);

                // 静态构造是return void
                Constructor.AddRet();

                // 生成所有函数
                FuncStmt funcStmt = classStmt.Methods;
                while (funcStmt != null)
                {
                    methodEnumerator.MoveNext();

                    MethodCodeGen(funcStmt, methodEnumerator.Current);

                    funcStmt = (FuncStmt)funcStmt.SiblingAST;
                }

                root = root.SiblingAST;
            }

            return null;
        }

        private void MethodCodeGen(FuncStmt funcStmt, Method method)
        {
            // 函数局部变量栈
            LocalSymbolTable = new SymbolTable();
            LocalSymbolTable.PushFrame();

            // 将参数加入符号表
            VarStmt param = funcStmt.Params.Params;
            if (method.AccessFlag.IsStatic)
            {
                foreach (Variable p in method.Params)
                {
                    LocalSymbolTable.AddSymbol(param.Id, p);
                    param = (VarStmt)param.SiblingAST;
                }
            }
            else
            {
                // 非静态有一个this
                LocalSymbolTable.AddSymbol("this", method.Params[0]);
                for (int i = 1; i < method.Params.Count; ++i)
                {
                    LocalSymbolTable.AddSymbol(param.Id, method.Params[i]);
                    param = (VarStmt)param.SiblingAST;
                }
            }

            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(method);
            if (funcStmt.Id == "(init)")
            {
                NonStaticFields.TryGetValue(method.Parent, out List<(VarStmt, ClassField)> nonStaticFields);
                foreach ((var varStmt, var field) in nonStaticFields)
                {
                    if (varStmt.Init != null)
                    {
                        VariableType variableType = varStmt.Init.CodeGen(this);
                        Constructor.AddGetFieldAddress(field);
                        Constructor.AddAStoreT(variableType);
                        Constructor.AddPop(variableType);
                    }
                    // XiVM类变量默认全0的
                }
            }
            // 不要直接Body.CodeGen()，因为那样会新建一个NS
            AST.CodeGen(this, funcStmt.Body.Child);

            // 要检查XirGenPass.ModuleConstructor.CurrentBasicBlock最后一条Instruction是不是ret
            if (Constructor.CurrentBasicBlock.Instructions.Last?.Value.IsRet != true)
            {
                // 如果最后一条不是return
                if (method.Declaration.ReturnType == null)
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
        }
    }
}
