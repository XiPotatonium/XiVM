using System.Collections.Generic;
using XiLang.Errors;
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

        public override VariableType CodeGen()
        {
            // 参数类型信息
            List<VariableType> paramsType = new List<VariableType>();
            VarStmt param = Params.Params;
            while (param != null)
            {
                VariableType paramType = param.Type.ToXirType();
                paramsType.Add(paramType);
                param = (VarStmt)param.SiblingAST;
            }

            // 构造函数
            FunctionType functionType = new FunctionType(Type.ToXirType(), paramsType);
            Function function = CodeGenPass.Constructor.AddFunction(Id, functionType);

            // 函数局部变量栈帧
            CodeGenPass.Constructor.SymbolTable.Push();

            // 形参
            param = Params.Params;
            int i = 0;
            while (param != null)
            {
                CodeGenPass.Constructor.SetFunctionParamName(function.Variables[i], param.Id);
                param = (VarStmt)param.SiblingAST;
                ++i;
            }

            // 不要直接CodeGen Body，因为那样会新建一个NS
            CodeGen(Body.Child);

            // 要检查XirGenPass.ModuleConstructor.CurrentBasicBlock最后一条Instruction是不是ret
            if (CodeGenPass.Constructor.CurrentBasicBlock.Instructions.Count == 0 ||
                !Instruction.IsReturn(CodeGenPass.Constructor.CurrentBasicBlock.Instructions.Last?.Value))
            {
                // 如果最后一条不是return
                if (functionType.ReturnType == null)
                {
                    // 如果函数返回void，自动补上ret
                    CodeGenPass.Constructor.AddRetT(null);
                }
                else
                {
                    // 否则报错
                    throw new XiLangError($"Function {param.Id} should return a value.");
                }
            }

            CodeGenPass.Constructor.SymbolTable.Pop();
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

        public override VariableType CodeGen()
        {
            throw new System.NotImplementedException();
        }
    }
}
