using XiLang.Errors;
using XiVM;
using XiVM.Xir.Symbol;

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
            // 查找函数声明
            Constructor.SymbolTable.TryGetValue(Id, out Symbol symbol);
            Function function = ((FunctionSymbol)symbol).Function;

            if (function.BasicBlocks.Count != 0)
            {
                throw new XiLangError($"Redefinition of function {function.Name}");
            }

            // 函数局部变量栈帧
            Constructor.SymbolTable.Push();

            // 将参数加入符号表
            VarStmt param = Params.Params;
            foreach (Variable p in function.Params)
            {
                // 将参数加入符号表
                Constructor.SetFunctionParamName(p, param.Id);
                param = (VarStmt)param.SiblingAST;
            }

            Constructor.CurrentBasicBlock = Constructor.AddBasicBlock(function);
            // 不要直接CodeGen Body，因为那样会新建一个NS
            CodeGen(Body.Child);

            // 要检查XirGenPass.ModuleConstructor.CurrentBasicBlock最后一条Instruction是不是ret
            if (Constructor.CurrentBasicBlock.Value.Instructions.Last?.Value.IsRet != true)
            {
                // 如果最后一条不是return
                if (function.Type.ReturnType == null)
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

            Constructor.SymbolTable.Pop();
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
