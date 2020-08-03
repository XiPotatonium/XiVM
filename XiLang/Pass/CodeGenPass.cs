using System;
using System.Collections.Generic;
using XiLang.AbstractSyntaxTree;
using XiVM;
using XiVM.Xir;

namespace XiLang.Pass
{
    internal class CodeGenPass : IASTPass
    {
        public static CodeGenPass Singleton { get; } = new CodeGenPass();

        public static ModuleConstructor Constructor => Program.ModuleConstructor;

        public static Stack<BasicBlock> Breakable { private set; get; } = new Stack<BasicBlock>();
        public static Stack<BasicBlock> Continuable { private set; get; } = new Stack<BasicBlock>();

        private CodeGenPass() { }

        public object Run(AST root)
        {
            // TODO 对于顶层需要进行特殊处理
            // 所有的Class、Function均需要建立但是不生成内部细节
            // 等于是先进行默认的声明
            AST cur = root;
            while (cur != null)
            {
                if (cur is ClassStmt)
                {
                    GlobalCodeGenClass((ClassStmt)cur);
                }
                else if (cur is FuncStmt)
                {
                    GlobalCodeGenFunction((FuncStmt)cur);
                }
                else if (cur is VarStmt)
                {
                    GlobalCodeGenVariable((VarStmt)cur);
                }
                else
                {
                    throw new NotImplementedException();
                }
                cur = cur.SiblingAST;
            }

            cur = root;
            while (cur != null)
            {
                if (cur is ClassStmt)
                {
                    throw new NotImplementedException();
                }
                else if (cur is FuncStmt)
                {
                    cur.CodeGen();
                }
                else if (cur is VarStmt)
                {
                    // 跳过全局变量，已经生成过了
                }
                else
                {
                    throw new NotImplementedException();
                }
                cur = cur.SiblingAST;
            }

            return null;
        }

        private void GlobalCodeGenClass(ClassStmt classStmt)
        {
            throw new NotImplementedException();
        }

        private void GlobalCodeGenFunction(FuncStmt funcStmt)
        {
            // 参数类型信息
            List<VariableType> paramsType = new List<VariableType>();
            VarStmt param = funcStmt.Params.Params;
            while (param != null)
            {
                VariableType paramType = param.Type.ToXirType();
                paramsType.Add(paramType);
                param = (VarStmt)param.SiblingAST;
            }

            // 函数
            FunctionType functionType = new FunctionType(funcStmt.Type.ToXirType(), paramsType);
            Function function = Constructor.AddFunction(funcStmt.Id, functionType);

            if (Constructor.SymbolTable.Count == 1 && funcStmt.Id == "main")
            {
                // 是main函数
                Constructor.MainFunction = function;
            }
        }

        private void GlobalCodeGenVariable(VarStmt varStmt)
        {
            Variable var = Constructor.AddLocalVariable(varStmt.Id, varStmt.Type.ToXirType());

            if (varStmt.Init != null)
            {   // 初始化代码
                varStmt.Init.CodeGen();
            }
            else
            {   // 没有初始化，全局变量有默认初始化
                switch (var.Type.Tag)
                {
                    case VariableTypeTag.BYTE:
                        Constructor.AddPushB(0);
                        break;
                    case VariableTypeTag.INT:
                        Constructor.AddPushI(0);
                        break;
                    case VariableTypeTag.DOUBLE:
                        Constructor.AddPushD(0);
                        break;
                    case VariableTypeTag.ADDRESS:
                        Constructor.AddPushA(0);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            Constructor.AddLocalA(var.Offset);      // addr
            Constructor.AddStoreT(var.Type);        // store
        }
    }
}
