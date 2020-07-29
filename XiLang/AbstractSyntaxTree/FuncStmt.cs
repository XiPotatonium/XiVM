using System;
using System.Collections.Generic;
using XiLang.Errors;
using XiLang.Symbol;
using XiVM;
using XiVM.Xir;

namespace XiLang.AbstractSyntaxTree
{
    public class FuncStmt : DeclarationStmt
    {
        public BlockStmt Body { set; get; }
        public ParamsAst Params { private set; get; }

        public FuncStmt(TypeExpr type, string id, ParamsAst ps) : base(type, id)
        {
            Params = ps;
        }

        public override string ASTLabel()
        {
            if (Body == null)
            {   // 函数声明
                return $"(FuncDecl){Id}";
            }
            return $"(FuncDef){Id}";
        }

        public override AST[] Children()
        {
            return new AST[] { Type, Params, Body };
        }

        public override XirValue CodeGen()
        {
            XirGenPass.VariableSymbolTable.Push();

            // 参数类型信息以及形参的构建
            List<XirType> paramsType = new List<XirType>();
            VarStmt param = Params.Params;
            while (param != null)
            {
                XirType paramType = param.Type.ToXirType();
                paramsType.Add(paramType);
                VariableSymbol variableSymbol = new VariableSymbol(param.Id, XirGenPass.ModuleConstructor.AddVariable(paramType));
                XirGenPass.VariableSymbolTable.Add(param.Id, variableSymbol);

                param = (VarStmt)param.SiblingAST;
            }

            FunctionType functionType = new FunctionType(Type.ToXirType(), paramsType);

            Function function = XirGenPass.ModuleConstructor.AddFunction(Id, functionType);
            BasicBlock entryBB = XirGenPass.ModuleConstructor.AddBasicBlock(function);
            XirGenPass.ModuleConstructor.CurrentBasicBlock = entryBB;

            // 不要直接CodeGen Body，因为那样会新建一个NS
            CodeGen(Body.Child);

            // TODO 要检查XirGenPass.ModuleConstructor.CurrentBasicBlock最后一条Instruction是不是ret
            if (XirGenPass.ModuleConstructor.CurrentBasicBlock.Instructions.Count == 0 ||
                !Instruction.IsReturn(XirGenPass.ModuleConstructor.CurrentBasicBlock.Instructions[^1]))
            {
                // 如果最后一条不是return
                if (functionType.ReturnType.Tag == XirTypeTag.VOID)
                {
                    // 如果函数返回void，自动补上ret
                    XirGenPass.ModuleConstructor.AddReturnInstruction(null);
                }
                else
                {
                    // 否则报错
                    throw new XiLangError($"Function {param.Id} should return a value.");
                }
            }

            XirGenPass.VariableSymbolTable.Pop();
            return null;
        }
    }

    public class ParamsAst : AST
    {
        public VarStmt Params { set; get; }

        public ParamsAst(VarStmt ps)
        {
            Params = ps;
        }

        public override string ASTLabel()
        {
            return "(Params)";
        }

        public override AST[] Children()
        {
            return new AST[] { Params };
        }

        public override XirValue CodeGen()
        {
            throw new System.NotImplementedException();
        }
    }
}
